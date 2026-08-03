using System.Text.Json;
using BikeService.Application.Interfaces;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;
using BikeService.Domain.Enums;
using MediatR;

namespace BikeService.Application.Features.Mechanics.Commands.DeleteMechanic;

public class DeleteMechanicCommandHandler(
    IUnitOfWork unitOfWork,
    IAuditLogService auditLogService,
    IUserContextService userContextService) : IRequestHandler<DeleteMechanicCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(DeleteMechanicCommand request, CancellationToken cancellationToken)
    {
        var entity = await unitOfWork.Repository<Mechanic>().GetByIdAsync(request.Id);
        if (entity == null)
            return Result<bool>.Fail("Mechanic not found.");

        var hasActiveTickets = await unitOfWork.Repository<ServiceTicket>()
            .AnyAsync(t => t.MechanicId == request.Id
                && t.Status != ServiceTicketStatus.Delivered
                && t.Status != ServiceTicketStatus.Cancelled);
        if (hasActiveTickets)
            return Result<bool>.Fail("Mechanic has active tickets and cannot be deleted.");

        var oldValues = JsonSerializer.Serialize(new
        {
            entity.FullName,
            entity.Specialty,
            entity.IsAvailable,
            entity.UserId
        });

        unitOfWork.Repository<Mechanic>().Remove(entity);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await auditLogService.LogAsync(
            "Mechanic", "Delete",
            userContextService.UserId, userContextService.Email,
            $"Deleted mechanic '{entity.FullName}'",
            entityId: request.Id.ToString(),
            ipAddress: userContextService.IpAddress,
            userAgent: userContextService.UserAgent,
            oldValues: oldValues,
            newValues: null);

        return Result<bool>.Ok(true, "Mechanic deleted successfully.");
    }
}
