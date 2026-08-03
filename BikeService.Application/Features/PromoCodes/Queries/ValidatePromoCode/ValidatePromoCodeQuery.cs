using BikeService.Application.DTOs.PromoCode;
using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.PromoCodes.Queries.ValidatePromoCode;

public record ValidatePromoCodeQuery(string Code) : IRequest<Result<PromoCodeDto>>;
