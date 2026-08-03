using System.Text.Json;
using BikeService.Application.Interfaces;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;
using MediatR;

namespace BikeService.Application.Features.Mechanics.Commands.UpdateMechanic;

public class UpdateMechanicCommandHandler(
    IUnitOfWork unitOfWork,
    IAuditLogService auditLogService,
    IUserContextService userContextService) : IRequestHandler<UpdateMechanicCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(UpdateMechanicCommand request, CancellationToken cancellationToken)
    {
        var entity = await unitOfWork.Repository<Mechanic>().GetByIdAsync(request.Id);
        if (entity == null)
            return Result<bool>.Fail("Mechanic not found.");

        var duplicate = await unitOfWork.Repository<Mechanic>()
            .AnyAsync(e => e.FullName == request.FullName && e.Id != request.Id);
        if (duplicate)
            return Result<bool>.FailField("FullName", "A mechanic with this name already exists.");

        var oldValues = JsonSerializer.Serialize(new
        {
            entity.FullName,
            entity.Specialty,
            entity.IsAvailable,
            entity.UserId
        });

        entity.FullName = request.FullName;
        entity.Specialty = request.Specialty;
        entity.IsAvailable = request.IsAvailable;
        entity.UpdatedBy = userContextService.UserId;
        entity.UpdatedAt = DateTime.UtcNow;

        unitOfWork.Repository<Mechanic>().Update(entity);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await auditLogService.LogAsync(
            "Mechanic", "Update",
            userContextService.UserId, userContextService.Email,
            $"Updated mechanic '{entity.FullName}'",
            entityId: entity.Id.ToString(),
            ipAddress: userContextService.IpAddress,
            userAgent: userContextService.UserAgent,
            oldValues: oldValues,
            newValues: JsonSerializer.Serialize(new
            {
                entity.FullName,
                entity.Specialty,
                entity.IsAvailable,
                entity.UserId
            }));

        return Result<bool>.Ok(true, "Mechanic updated successfully.");
    }
}
