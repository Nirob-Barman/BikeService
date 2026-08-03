using BikeService.Application.DTOs.PromoCode;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Mappers;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;
using MediatR;

namespace BikeService.Application.Features.PromoCodes.Queries.ValidatePromoCode;

public class ValidatePromoCodeQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<ValidatePromoCodeQuery, Result<PromoCodeDto>>
{
    public async Task<Result<PromoCodeDto>> Handle(ValidatePromoCodeQuery request, CancellationToken cancellationToken)
    {
        var entity = await unitOfWork.Repository<PromoCode>()
            .FirstOrDefaultAsync(e => e.Code == request.Code);

        if (entity == null)
            return Result<PromoCodeDto>.Fail("Promo code not found.");

        if (!entity.IsActive)
            return Result<PromoCodeDto>.Fail("This promo code is no longer active.");

        if (entity.ExpiresAt.HasValue && entity.ExpiresAt.Value < DateTime.UtcNow)
            return Result<PromoCodeDto>.Fail("This promo code has expired.");

        if (entity.UsageCount >= entity.MaxUsages)
            return Result<PromoCodeDto>.Fail("This promo code has reached its maximum usage limit.");

        return Result<PromoCodeDto>.Ok(PromoCodeMapper.ToDto(entity), "Promo code is valid.");
    }
}
