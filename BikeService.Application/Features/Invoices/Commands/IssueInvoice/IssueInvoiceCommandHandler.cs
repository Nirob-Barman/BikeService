using System.Text.Json;
using BikeService.Application.Interfaces;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;
using BikeService.Domain.Enums;
using MediatR;

namespace BikeService.Application.Features.Invoices.Commands.IssueInvoice;

public class IssueInvoiceCommandHandler(
    IUnitOfWork unitOfWork,
    IAuditLogService auditLogService,
    IUserContextService userContextService,
    INotificationService notificationService) : IRequestHandler<IssueInvoiceCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(IssueInvoiceCommand request, CancellationToken cancellationToken)
    {
        var invoice = await unitOfWork.Repository<Invoice>().GetByIdAsync(request.Id);
        if (invoice == null)
            return Result<bool>.Fail("Invoice not found.");

        if (invoice.Status != InvoiceStatus.Draft)
            return Result<bool>.Fail("Only draft invoices can be issued.");

        var oldValues = JsonSerializer.Serialize(new { Status = invoice.Status.ToString() });

        invoice.Status = InvoiceStatus.Issued;
        invoice.UpdatedBy = userContextService.UserId;
        invoice.UpdatedAt = DateTime.UtcNow;

        unitOfWork.Repository<Invoice>().Update(invoice);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await auditLogService.LogAsync(
            "Invoice", "Issue",
            userContextService.UserId, userContextService.Email,
            $"Issued invoice ID {request.Id}",
            entityId: request.Id.ToString(),
            ipAddress: userContextService.IpAddress,
            userAgent: userContextService.UserAgent,
            oldValues: oldValues,
            newValues: JsonSerializer.Serialize(new { Status = InvoiceStatus.Issued.ToString() }));

        var ticket = await unitOfWork.Repository<ServiceTicket>().GetByIdAsync(invoice.ServiceTicketId);
        if (ticket != null)
        {
            var bike = await unitOfWork.Repository<CustomerBike>().GetByIdAsync(ticket.BikeId);
            if (bike != null && !string.IsNullOrEmpty(bike.CustomerId))
            {
                await notificationService.CreateNotificationAsync(
                    bike.CustomerId,
                    "Invoice Ready",
                    $"Your invoice (#{request.Id}) is ready. Please review and complete payment.",
                    link: $"/Invoice/Detail/{request.Id}");
            }
        }

        return Result<bool>.Ok(true, "Invoice issued successfully.");
    }
}
