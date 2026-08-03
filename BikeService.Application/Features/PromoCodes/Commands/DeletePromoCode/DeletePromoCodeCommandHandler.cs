using System.Text.Json;
using BikeService.Application.Interfaces;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;
using MediatR;

namespace BikeService.Application.Features.PromoCodes.Commands.DeletePromoCode;

public class DeletePromoCodeCommandHandler(
    IUnitOfWork unitOfWork,
    IAuditLogService auditLogService,
    IUserContextService userContextService) : IRequestHandler<DeletePromoCodeCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(DeletePromoCodeCommand request, CancellationToken cancellationToken)
    {
        var entity = await unitOfWork.Repository<PromoCode>().GetByIdAsync(request.Id);
        if (entity == null)
            return Result<bool>.Fail("Promo code not found.");

        if (entity.UsageCount > 0)
            return Result<bool>.Fail("Cannot delete a promo code that has already been used.");

        var oldValues = JsonSerializer.Serialize(new
        {
            entity.Code,
            entity.DiscountPercent,
            entity.MaxUsages,
            entity.UsageCount,
            entity.ExpiresAt,
            entity.IsActive
        });

        unitOfWork.Repository<PromoCode>().Remove(entity);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await auditLogService.LogAsync(
            "PromoCode", "Delete",
            userContextService.UserId, userContextService.Email,
            $"Deleted promo code '{entity.Code}'",
            entityId: request.Id.ToString(),
            ipAddress: userContextService.IpAddress,
            userAgent: userContextService.UserAgent,
            oldValues: oldValues,
            newValues: null);

        return Result<bool>.Ok(true, "Promo code deleted successfully.");
    }
}
