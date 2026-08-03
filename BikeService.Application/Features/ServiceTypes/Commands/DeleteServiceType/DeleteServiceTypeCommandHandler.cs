using System.Text.Json;
using BikeService.Application.Interfaces;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;
using MediatR;

namespace BikeService.Application.Features.ServiceTypes.Commands.DeleteServiceType;

public class DeleteServiceTypeCommandHandler(
    IUnitOfWork unitOfWork,
    IAuditLogService auditLogService,
    IUserContextService userContextService) : IRequestHandler<DeleteServiceTypeCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(DeleteServiceTypeCommand request, CancellationToken cancellationToken)
    {
        var entity = await unitOfWork.Repository<ServiceType>().GetByIdAsync(request.Id);
        if (entity == null)
            return Result<bool>.Fail("Service type not found.");

        var hasItems = await unitOfWork.Repository<ServiceTicketItem>()
            .AnyAsync(i => i.ServiceTypeId == request.Id);
        if (hasItems)
            return Result<bool>.Fail("Cannot delete this service type because it is referenced by existing service ticket items.");

        var oldValues = JsonSerializer.Serialize(new
        {
            entity.Name,
            entity.Description,
            entity.BasePrice,
            entity.EstimatedHours,
            entity.IsActive
        });

        unitOfWork.Repository<ServiceType>().Remove(entity);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await auditLogService.LogAsync(
            "ServiceType", "Delete",
            userContextService.UserId, userContextService.Email,
            $"Deleted service type '{entity.Name}'",
            entityId: request.Id.ToString(),
            ipAddress: userContextService.IpAddress,
            userAgent: userContextService.UserAgent,
            oldValues: oldValues,
            newValues: null);

        return Result<bool>.Ok(true, "Service type deleted successfully.");
    }
}
