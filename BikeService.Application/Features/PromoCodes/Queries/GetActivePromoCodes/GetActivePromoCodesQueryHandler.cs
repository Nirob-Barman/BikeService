using BikeService.Application.DTOs.PromoCode;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Mappers;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;
using MediatR;

namespace BikeService.Application.Features.PromoCodes.Queries.GetActivePromoCodes;

public class GetActivePromoCodesQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetActivePromoCodesQuery, Result<List<PromoCodeDto>>>
{
    public async Task<Result<List<PromoCodeDto>>> Handle(GetActivePromoCodesQuery request, CancellationToken cancellationToken)
    {
        var items = await unitOfWork.Repository<PromoCode>()
            .GetAllAsync<PromoCodeDto>(e => e.IsActive, e => PromoCodeMapper.ToDto(e));
        return Result<List<PromoCodeDto>>.Ok(items.ToList());
    }
}
