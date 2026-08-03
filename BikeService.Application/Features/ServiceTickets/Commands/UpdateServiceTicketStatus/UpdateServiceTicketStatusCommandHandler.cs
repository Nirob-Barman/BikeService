using System.Text.Json;
using BikeService.Application.Interfaces;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;
using BikeService.Domain.Enums;
using MediatR;

namespace BikeService.Application.Features.ServiceTickets.Commands.UpdateServiceTicketStatus;

public class UpdateServiceTicketStatusCommandHandler(
    IUnitOfWork unitOfWork,
    IAuditLogService auditLogService,
    IUserContextService userContextService,
    INotificationService notificationService) : IRequestHandler<UpdateServiceTicketStatusCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(UpdateServiceTicketStatusCommand request, CancellationToken cancellationToken)
    {
        var ticket = await unitOfWork.Repository<ServiceTicket>().GetByIdAsync(request.Id);
        if (ticket == null)
            return Result<bool>.Fail("Service ticket not found.");

        var currentStatus = ticket.Status;
        var newStatus = request.NewStatus;

        if (newStatus == ServiceTicketStatus.Cancelled)
        {
            if (currentStatus == ServiceTicketStatus.Delivered)
                return Result<bool>.Fail("Cannot cancel a delivered ticket.");
        }
        else
        {
            var validNext = GetNextStatus(currentStatus);
            if (validNext == null || validNext.Value != newStatus)
                return Result<bool>.Fail($"Invalid status transition from {currentStatus} to {newStatus}.");
        }

        var oldValues = JsonSerializer.Serialize(new { Status = currentStatus.ToString() });

        if (newStatus == ServiceTicketStatus.InProgress)
        {
            await unitOfWork.BeginTransaction();
            try
            {
                await DeductPartsStockAsync(request.Id);
                ticket.Status = newStatus;
                ticket.UpdatedBy = userContextService.UserId;
                ticket.UpdatedAt = DateTime.UtcNow;
                unitOfWork.Repository<ServiceTicket>().Update(ticket);
                await unitOfWork.SaveChangesAsync(cancellationToken);
                await unitOfWork.CommitAsync();
            }
            catch
            {
                await unitOfWork.RollbackAsync();
                throw;
            }
        }
        else if (newStatus == ServiceTicketStatus.Cancelled &&
                 (currentStatus == ServiceTicketStatus.InProgress ||
                  currentStatus == ServiceTicketStatus.QualityCheck ||
                  currentStatus == ServiceTicketStatus.ReadyForPickup))
        {
            await unitOfWork.BeginTransaction();
            try
            {
                await RestockPartsAsync(request.Id);
                ticket.Status = newStatus;
                ticket.UpdatedBy = userContextService.UserId;
                ticket.UpdatedAt = DateTime.UtcNow;
                unitOfWork.Repository<ServiceTicket>().Update(ticket);
                await unitOfWork.SaveChangesAsync(cancellationToken);
                await unitOfWork.CommitAsync();
            }
            catch
            {
                await unitOfWork.RollbackAsync();
                throw;
            }
        }
        else
        {
            ticket.Status = newStatus;
            ticket.UpdatedBy = userContextService.UserId;
            ticket.UpdatedAt = DateTime.UtcNow;
            unitOfWork.Repository<ServiceTicket>().Update(ticket);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        await auditLogService.LogAsync(
            "ServiceTicket", "StatusUpdate",
            userContextService.UserId, userContextService.Email,
            $"Status changed from {currentStatus} to {newStatus} for ticket ID {request.Id}",
            entityId: request.Id.ToString(),
            ipAddress: userContextService.IpAddress,
            userAgent: userContextService.UserAgent,
            oldValues: oldValues,
            newValues: JsonSerializer.Serialize(new { Status = newStatus.ToString() }));

        if (newStatus == ServiceTicketStatus.ReadyForPickup)
        {
            var bikeRecord = await unitOfWork.Repository<CustomerBike>().GetByIdAsync(ticket.BikeId);
            if (bikeRecord != null && !string.IsNullOrEmpty(bikeRecord.CustomerId))
            {
                await notificationService.CreateNotificationAsync(
                    bikeRecord.CustomerId,
                    "Bike Ready for Pickup",
                    $"Your bike service (ticket #{request.Id}) is complete and ready for pickup.",
                    link: $"/ServiceTicket/Detail/{request.Id}");
            }
        }

        return Result<bool>.Ok(true, $"Status updated to {newStatus}.");
    }

    private static ServiceTicketStatus? GetNextStatus(ServiceTicketStatus current) => current switch
    {
        ServiceTicketStatus.Pending => ServiceTicketStatus.Diagnosed,
        ServiceTicketStatus.Diagnosed => ServiceTicketStatus.InProgress,
        ServiceTicketStatus.InProgress => ServiceTicketStatus.QualityCheck,
        ServiceTicketStatus.QualityCheck => ServiceTicketStatus.ReadyForPickup,
        ServiceTicketStatus.ReadyForPickup => ServiceTicketStatus.Delivered,
        _ => null
    };

    private async Task DeductPartsStockAsync(int ticketId)
    {
        var items = await unitOfWork.Repository<ServiceTicketItem>()
            .Where(i => i.ServiceTicketId == ticketId && i.PartId.HasValue);

        foreach (var item in items)
        {
            var part = await unitOfWork.Repository<Part>().GetByIdAsync(item.PartId!.Value);
            if (part == null) continue;
            part.StockQuantity -= item.Quantity;
            if (part.StockQuantity < 0) part.StockQuantity = 0;
            unitOfWork.Repository<Part>().Update(part);
        }
    }

    private async Task RestockPartsAsync(int ticketId)
    {
        var items = await unitOfWork.Repository<ServiceTicketItem>()
            .Where(i => i.ServiceTicketId == ticketId && i.PartId.HasValue);

        foreach (var item in items)
        {
            var part = await unitOfWork.Repository<Part>().GetByIdAsync(item.PartId!.Value);
            if (part == null) continue;
            part.StockQuantity += item.Quantity;
            unitOfWork.Repository<Part>().Update(part);
        }
    }
}
