using System.Text.Json;
using BikeService.Application.Interfaces;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;
using BikeService.Domain.Enums;
using MediatR;

namespace BikeService.Application.Features.ServiceTickets.Commands.RemoveServiceTicketItem;

public class RemoveServiceTicketItemCommandHandler(
    IUnitOfWork unitOfWork,
    IAuditLogService auditLogService,
    IUserContextService userContextService) : IRequestHandler<RemoveServiceTicketItemCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(RemoveServiceTicketItemCommand request, CancellationToken cancellationToken)
    {
        var item = await unitOfWork.Repository<ServiceTicketItem>().GetByIdAsync(request.ItemId);
        if (item == null)
            return Result<bool>.Fail("Item not found.");

        var ticket = await unitOfWork.Repository<ServiceTicket>().GetByIdAsync(item.ServiceTicketId);
        if (ticket != null &&
            (ticket.Status == ServiceTicketStatus.Delivered || ticket.Status == ServiceTicketStatus.Cancelled))
            return Result<bool>.Fail("Cannot remove items from a delivered or cancelled ticket.");

        var oldValues = JsonSerializer.Serialize(new
        {
            item.ServiceTicketId,
            item.ServiceTypeId,
            item.PartId,
            item.Quantity,
            item.UnitPrice
        });

        unitOfWork.Repository<ServiceTicketItem>().Remove(item);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await auditLogService.LogAsync(
            "ServiceTicketItem", "Delete",
            userContextService.UserId, userContextService.Email,
            $"Removed item ID {request.ItemId} from ticket ID {item.ServiceTicketId}",
            entityId: request.ItemId.ToString(),
            ipAddress: userContextService.IpAddress,
            userAgent: userContextService.UserAgent,
            oldValues: oldValues,
            newValues: null);

        return Result<bool>.Ok(true, "Item removed successfully.");
    }
}
