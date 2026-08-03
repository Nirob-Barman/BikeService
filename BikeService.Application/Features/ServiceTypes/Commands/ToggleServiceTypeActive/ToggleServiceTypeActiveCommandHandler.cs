using System.Text.Json;
using BikeService.Application.Interfaces;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;
using MediatR;

namespace BikeService.Application.Features.ServiceTypes.Commands.ToggleServiceTypeActive;

public class ToggleServiceTypeActiveCommandHandler(
    IUnitOfWork unitOfWork,
    IAuditLogService auditLogService,
    IUserContextService userContextService) : IRequestHandler<ToggleServiceTypeActiveCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(ToggleServiceTypeActiveCommand request, CancellationToken cancellationToken)
    {
        var entity = await unitOfWork.Repository<ServiceType>().GetByIdAsync(request.Id);
        if (entity == null)
            return Result<bool>.Fail("Service type not found.");

        var oldValues = JsonSerializer.Serialize(new { entity.IsActive });

        entity.IsActive = !entity.IsActive;
        entity.UpdatedBy = userContextService.UserId;
        entity.UpdatedAt = DateTime.UtcNow;

        unitOfWork.Repository<ServiceType>().Update(entity);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await auditLogService.LogAsync(
            "ServiceType", "Toggle",
            userContextService.UserId, userContextService.Email,
            $"Toggled service type '{entity.Name}' IsActive to {entity.IsActive}",
            entityId: entity.Id.ToString(),
            ipAddress: userContextService.IpAddress,
            userAgent: userContextService.UserAgent,
            oldValues: oldValues,
            newValues: JsonSerializer.Serialize(new { entity.IsActive }));

        return Result<bool>.Ok(true, $"Service type is now {(entity.IsActive ? "active" : "inactive")}.");
    }
}
