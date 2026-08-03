using System.Text.Json;
using BikeService.Application.Interfaces;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;
using BikeService.Domain.Enums;
using MediatR;

namespace BikeService.Application.Features.ServiceTickets.Commands.AddServiceTicketItem;

public class AddServiceTicketItemCommandHandler(
    IUnitOfWork unitOfWork,
    IAuditLogService auditLogService,
    IUserContextService userContextService) : IRequestHandler<AddServiceTicketItemCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(AddServiceTicketItemCommand request, CancellationToken cancellationToken)
    {
        if (!request.ServiceTypeId.HasValue && !request.PartId.HasValue)
            return Result<bool>.Fail("Either a service type or a part must be specified.");

        if (request.Quantity < 1)
            return Result<bool>.FailField("Quantity", "Quantity must be at least 1.");

        var ticket = await unitOfWork.Repository<ServiceTicket>().GetByIdAsync(request.TicketId);
        if (ticket == null)
            return Result<bool>.Fail("Service ticket not found.");

        if (ticket.Status == ServiceTicketStatus.Delivered || ticket.Status == ServiceTicketStatus.Cancelled)
            return Result<bool>.Fail("Cannot add items to a delivered or cancelled ticket.");

        decimal unitPrice = request.UnitPrice;

        if (request.PartId.HasValue)
        {
            var part = await unitOfWork.Repository<Part>().GetByIdAsync(request.PartId.Value);
            if (part == null)
                return Result<bool>.Fail("Part not found.");
            if (part.StockQuantity < request.Quantity)
                return Result<bool>.Fail($"Insufficient stock. Available: {part.StockQuantity}.");
            if (unitPrice == 0)
                unitPrice = part.UnitPrice;
        }

        if (request.ServiceTypeId.HasValue)
        {
            var serviceType = await unitOfWork.Repository<ServiceType>().GetByIdAsync(request.ServiceTypeId.Value);
            if (serviceType == null)
                return Result<bool>.Fail("Service type not found.");
            if (unitPrice == 0)
                unitPrice = serviceType.BasePrice;
        }

        var item = new ServiceTicketItem
        {
            ServiceTicketId = request.TicketId,
            ServiceTypeId = request.ServiceTypeId,
            PartId = request.PartId,
            Quantity = request.Quantity,
            UnitPrice = unitPrice,
            CreatedBy = userContextService.UserId,
            CreatedAt = DateTime.UtcNow
        };

        await unitOfWork.Repository<ServiceTicketItem>().AddAsync(item);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await auditLogService.LogAsync(
            "ServiceTicketItem", "Create",
            userContextService.UserId, userContextService.Email,
            $"Added item to ticket ID {request.TicketId}",
            entityId: item.Id.ToString(),
            ipAddress: userContextService.IpAddress,
            userAgent: userContextService.UserAgent,
            oldValues: null,
            newValues: JsonSerializer.Serialize(new
            {
                item.ServiceTicketId,
                item.ServiceTypeId,
                item.PartId,
                item.Quantity,
                item.UnitPrice
            }));

        return Result<bool>.Ok(true, "Item added successfully.");
    }
}
