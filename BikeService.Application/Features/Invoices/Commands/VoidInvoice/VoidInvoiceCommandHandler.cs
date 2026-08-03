using System.Text.Json;
using BikeService.Application.Interfaces;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;
using BikeService.Domain.Enums;
using MediatR;

namespace BikeService.Application.Features.Invoices.Commands.VoidInvoice;

public class VoidInvoiceCommandHandler(
    IUnitOfWork unitOfWork,
    IAuditLogService auditLogService,
    IUserContextService userContextService) : IRequestHandler<VoidInvoiceCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(VoidInvoiceCommand request, CancellationToken cancellationToken)
    {
        var invoice = await unitOfWork.Repository<Invoice>().GetByIdAsync(request.Id);
        if (invoice == null)
            return Result<bool>.Fail("Invoice not found.");

        if (invoice.Status == InvoiceStatus.Paid)
            return Result<bool>.Fail("Cannot void a paid invoice.");

        if (invoice.Status == InvoiceStatus.Void)
            return Result<bool>.Fail("Invoice is already voided.");

        var oldValues = JsonSerializer.Serialize(new { Status = invoice.Status.ToString() });

        invoice.Status = InvoiceStatus.Void;
        invoice.UpdatedBy = userContextService.UserId;
        invoice.UpdatedAt = DateTime.UtcNow;

        unitOfWork.Repository<Invoice>().Update(invoice);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await auditLogService.LogAsync(
            "Invoice", "Void",
            userContextService.UserId, userContextService.Email,
            $"Voided invoice ID {request.Id}",
            entityId: request.Id.ToString(),
            ipAddress: userContextService.IpAddress,
            userAgent: userContextService.UserAgent,
            oldValues: oldValues,
            newValues: JsonSerializer.Serialize(new { Status = InvoiceStatus.Void.ToString() }));

        return Result<bool>.Ok(true, "Invoice voided successfully.");
    }
}
