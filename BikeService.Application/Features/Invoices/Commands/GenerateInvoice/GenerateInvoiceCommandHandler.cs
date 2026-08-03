using System.Text.Json;
using BikeService.Application.Interfaces;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;
using BikeService.Domain.Enums;
using MediatR;

namespace BikeService.Application.Features.Invoices.Commands.GenerateInvoice;

public class GenerateInvoiceCommandHandler(
    IUnitOfWork unitOfWork,
    IAuditLogService auditLogService,
    IUserContextService userContextService) : IRequestHandler<GenerateInvoiceCommand, Result<int>>
{
    private const decimal TaxRate = 0.15m;

    public async Task<Result<int>> Handle(GenerateInvoiceCommand request, CancellationToken cancellationToken)
    {
        var existingInvoice = await unitOfWork.Repository<Invoice>()
            .AnyAsync(i => i.ServiceTicketId == request.TicketId);
        if (existingInvoice)
            return Result<int>.Fail("An invoice already exists for this ticket.");

        var ticketWithItems = await unitOfWork.Repository<ServiceTicket>()
            .GetAllWithIncludesAsync<ServiceTicket>(
                t => t.Id == request.TicketId,
                t => t,
                t => t.Items,
                t => t.Bike);

        var ticket = ticketWithItems.FirstOrDefault();
        if (ticket == null)
            return Result<int>.Fail("Service ticket not found.");

        var totalAmount = ticket.Items.Sum(i => i.Quantity * i.UnitPrice);
        var taxAmount = Math.Round(totalAmount * TaxRate, 2);
        var finalAmount = totalAmount + taxAmount;

        var invoice = new Invoice
        {
            ServiceTicketId = request.TicketId,
            TotalAmount = totalAmount,
            TaxAmount = taxAmount,
            DiscountAmount = 0,
            FinalAmount = finalAmount,
            Status = InvoiceStatus.Draft,
            CreatedBy = userContextService.UserId,
            CreatedAt = DateTime.UtcNow
        };

        await unitOfWork.Repository<Invoice>().AddAsync(invoice);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await auditLogService.LogAsync(
            "Invoice", "Create",
            userContextService.UserId, userContextService.Email,
            $"Generated invoice for ticket ID {request.TicketId}",
            entityId: invoice.Id.ToString(),
            ipAddress: userContextService.IpAddress,
            userAgent: userContextService.UserAgent,
            oldValues: null,
            newValues: JsonSerializer.Serialize(new
            {
                invoice.ServiceTicketId,
                invoice.TotalAmount,
                invoice.TaxAmount,
                invoice.FinalAmount,
                Status = invoice.Status.ToString()
            }));

        return Result<int>.Ok(invoice.Id, "Invoice generated successfully.");
    }
}
