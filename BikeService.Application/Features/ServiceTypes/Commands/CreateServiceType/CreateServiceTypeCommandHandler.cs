using System.Text.Json;
using BikeService.Application.Interfaces;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;
using MediatR;

namespace BikeService.Application.Features.ServiceTypes.Commands.CreateServiceType;

public class CreateServiceTypeCommandHandler(
    IUnitOfWork unitOfWork,
    IAuditLogService auditLogService,
    IUserContextService userContextService) : IRequestHandler<CreateServiceTypeCommand, Result<int>>
{
    public async Task<Result<int>> Handle(CreateServiceTypeCommand request, CancellationToken cancellationToken)
    {
        var duplicate = await unitOfWork.Repository<ServiceType>()
            .AnyAsync(e => e.Name == request.Name);
        if (duplicate)
            return Result<int>.FailField("Name", "A service type with this name already exists.");

        var entity = new ServiceType
        {
            Name = request.Name,
            Description = request.Description,
            BasePrice = request.BasePrice,
            EstimatedHours = request.EstimatedHours,
            IsActive = request.IsActive,
            CreatedBy = userContextService.UserId,
            CreatedAt = DateTime.UtcNow
        };

        await unitOfWork.Repository<ServiceType>().AddAsync(entity);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await auditLogService.LogAsync(
            "ServiceType", "Create",
            userContextService.UserId, userContextService.Email,
            $"Created service type '{entity.Name}'",
            entityId: entity.Id.ToString(),
            ipAddress: userContextService.IpAddress,
            userAgent: userContextService.UserAgent,
            oldValues: null,
            newValues: JsonSerializer.Serialize(new
            {
                entity.Name,
                entity.Description,
                entity.BasePrice,
                entity.EstimatedHours,
                entity.IsActive
            }));

        return Result<int>.Ok(entity.Id, "Service type created successfully.");
    }
}
