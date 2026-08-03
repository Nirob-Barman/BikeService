using System.Text.Json;
using BikeService.Application.Interfaces;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;
using MediatR;

namespace BikeService.Application.Features.PromoCodes.Commands.UpdatePromoCode;

public class UpdatePromoCodeCommandHandler(
    IUnitOfWork unitOfWork,
    IAuditLogService auditLogService,
    IUserContextService userContextService) : IRequestHandler<UpdatePromoCodeCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(UpdatePromoCodeCommand request, CancellationToken cancellationToken)
    {
        var entity = await unitOfWork.Repository<PromoCode>().GetByIdAsync(request.Id);
        if (entity == null)
            return Result<bool>.Fail("Promo code not found.");

        var duplicate = await unitOfWork.Repository<PromoCode>()
            .AnyAsync(e => e.Code == request.Code && e.Id != request.Id);
        if (duplicate)
            return Result<bool>.FailField("Code", "A promo code with this code already exists.");

        var oldValues = JsonSerializer.Serialize(new
        {
            entity.Code,
            entity.DiscountPercent,
            entity.MaxUsages,
            entity.ExpiresAt,
            entity.IsActive
        });

        entity.Code = request.Code;
        entity.DiscountPercent = request.DiscountPercent;
        entity.MaxUsages = request.MaxUsages;
        entity.ExpiresAt = request.ExpiresAt;
        entity.IsActive = request.IsActive;
        entity.UpdatedBy = userContextService.UserId;
        entity.UpdatedAt = DateTime.UtcNow;

        unitOfWork.Repository<PromoCode>().Update(entity);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await auditLogService.LogAsync(
            "PromoCode", "Update",
            userContextService.UserId, userContextService.Email,
            $"Updated promo code '{entity.Code}'",
            entityId: entity.Id.ToString(),
            ipAddress: userContextService.IpAddress,
            userAgent: userContextService.UserAgent,
            oldValues: oldValues,
            newValues: JsonSerializer.Serialize(new
            {
                entity.Code,
                entity.DiscountPercent,
                entity.MaxUsages,
                entity.ExpiresAt,
                entity.IsActive
            }));

        return Result<bool>.Ok(true, "Promo code updated successfully.");
    }
}
