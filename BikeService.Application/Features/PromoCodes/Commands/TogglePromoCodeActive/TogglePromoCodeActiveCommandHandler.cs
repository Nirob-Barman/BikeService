using System.Text.Json;
using BikeService.Application.Interfaces;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;
using MediatR;

namespace BikeService.Application.Features.PromoCodes.Commands.TogglePromoCodeActive;

public class TogglePromoCodeActiveCommandHandler(
    IUnitOfWork unitOfWork,
    IAuditLogService auditLogService,
    IUserContextService userContextService) : IRequestHandler<TogglePromoCodeActiveCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(TogglePromoCodeActiveCommand request, CancellationToken cancellationToken)
    {
        var entity = await unitOfWork.Repository<PromoCode>().GetByIdAsync(request.Id);
        if (entity == null)
            return Result<bool>.Fail("Promo code not found.");

        var oldValues = JsonSerializer.Serialize(new { entity.IsActive });

        entity.IsActive = !entity.IsActive;
        entity.UpdatedBy = userContextService.UserId;
        entity.UpdatedAt = DateTime.UtcNow;

        unitOfWork.Repository<PromoCode>().Update(entity);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await auditLogService.LogAsync(
            "PromoCode", "Toggle",
            userContextService.UserId, userContextService.Email,
            $"Toggled promo code '{entity.Code}' IsActive to {entity.IsActive}",
            entityId: entity.Id.ToString(),
            ipAddress: userContextService.IpAddress,
            userAgent: userContextService.UserAgent,
            oldValues: oldValues,
            newValues: JsonSerializer.Serialize(new { entity.IsActive }));

        return Result<bool>.Ok(true, $"Promo code is now {(entity.IsActive ? "active" : "inactive")}.");
    }
}
