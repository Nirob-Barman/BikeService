using BikeService.Application.DTOs.PromoCode;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Mappers;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;
using MediatR;

namespace BikeService.Application.Features.PromoCodes.Queries.GetPromoCodeById;

public class GetPromoCodeByIdQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetPromoCodeByIdQuery, Result<PromoCodeDto>>
{
    public async Task<Result<PromoCodeDto>> Handle(GetPromoCodeByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await unitOfWork.Repository<PromoCode>().GetByIdAsync(request.Id);
        if (entity == null)
            return Result<PromoCodeDto>.Fail("Promo code not found.");
        return Result<PromoCodeDto>.Ok(PromoCodeMapper.ToDto(entity));
    }
}
