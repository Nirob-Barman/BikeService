using System.Text.Json;
using BikeService.Application.Interfaces;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;
using MediatR;

namespace BikeService.Application.Features.Mechanics.Commands.ToggleMechanicAvailability;

public class ToggleMechanicAvailabilityCommandHandler(
    IUnitOfWork unitOfWork,
    IAuditLogService auditLogService,
    IUserContextService userContextService) : IRequestHandler<ToggleMechanicAvailabilityCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(ToggleMechanicAvailabilityCommand request, CancellationToken cancellationToken)
    {
        var entity = await unitOfWork.Repository<Mechanic>().GetByIdAsync(request.Id);
        if (entity == null)
            return Result<bool>.Fail("Mechanic not found.");

        var oldValues = JsonSerializer.Serialize(new { entity.IsAvailable });

        entity.IsAvailable = !entity.IsAvailable;
        entity.UpdatedBy = userContextService.UserId;
        entity.UpdatedAt = DateTime.UtcNow;

        unitOfWork.Repository<Mechanic>().Update(entity);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await auditLogService.LogAsync(
            "Mechanic", "Toggle",
            userContextService.UserId, userContextService.Email,
            $"Toggled mechanic '{entity.FullName}' IsAvailable to {entity.IsAvailable}",
            entityId: entity.Id.ToString(),
            ipAddress: userContextService.IpAddress,
            userAgent: userContextService.UserAgent,
            oldValues: oldValues,
            newValues: JsonSerializer.Serialize(new { entity.IsAvailable }));

        return Result<bool>.Ok(true, $"Mechanic is now {(entity.IsAvailable ? "available" : "unavailable")}.");
    }
}
