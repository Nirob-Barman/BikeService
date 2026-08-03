using BikeService.Application.DTOs.PromoCode;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Mappers;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;
using MediatR;

namespace BikeService.Application.Features.PromoCodes.Queries.GetPromoCodes;

public class GetPromoCodesQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetPromoCodesQuery, Result<List<PromoCodeDto>>>
{
    public async Task<Result<List<PromoCodeDto>>> Handle(GetPromoCodesQuery request, CancellationToken cancellationToken)
    {
        var items = await unitOfWork.Repository<PromoCode>()
            .GetAllAsync<PromoCodeDto>(e => PromoCodeMapper.ToDto(e));
        return Result<List<PromoCodeDto>>.Ok(items.ToList());
    }
}
