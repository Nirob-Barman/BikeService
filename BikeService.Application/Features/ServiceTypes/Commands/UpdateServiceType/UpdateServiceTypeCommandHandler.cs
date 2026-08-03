using System.Text.Json;
using BikeService.Application.Interfaces;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;
using MediatR;

namespace BikeService.Application.Features.ServiceTypes.Commands.UpdateServiceType;

public class UpdateServiceTypeCommandHandler(
    IUnitOfWork unitOfWork,
    IAuditLogService auditLogService,
    IUserContextService userContextService) : IRequestHandler<UpdateServiceTypeCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(UpdateServiceTypeCommand request, CancellationToken cancellationToken)
    {
        var entity = await unitOfWork.Repository<ServiceType>().GetByIdAsync(request.Id);
        if (entity == null)
            return Result<bool>.Fail("Service type not found.");

        var duplicate = await unitOfWork.Repository<ServiceType>()
            .AnyAsync(e => e.Name == request.Name && e.Id != request.Id);
        if (duplicate)
            return Result<bool>.FailField("Name", "A service type with this name already exists.");

        var oldValues = JsonSerializer.Serialize(new
        {
            entity.Name,
            entity.Description,
            entity.BasePrice,
            entity.EstimatedHours,
            entity.IsActive
        });

        entity.Name = request.Name;
        entity.Description = request.Description;
        entity.BasePrice = request.BasePrice;
        entity.EstimatedHours = request.EstimatedHours;
        entity.IsActive = request.IsActive;
        entity.UpdatedBy = userContextService.UserId;
        entity.UpdatedAt = DateTime.UtcNow;

        unitOfWork.Repository<ServiceType>().Update(entity);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await auditLogService.LogAsync(
            "ServiceType", "Update",
            userContextService.UserId, userContextService.Email,
            $"Updated service type '{entity.Name}'",
            entityId: entity.Id.ToString(),
            ipAddress: userContextService.IpAddress,
            userAgent: userContextService.UserAgent,
            oldValues: oldValues,
            newValues: JsonSerializer.Serialize(new
            {
                entity.Name,
                entity.Description,
                entity.BasePrice,
                entity.EstimatedHours,
                entity.IsActive
            }));

        return Result<bool>.Ok(true, "Service type updated successfully.");
    }
}
