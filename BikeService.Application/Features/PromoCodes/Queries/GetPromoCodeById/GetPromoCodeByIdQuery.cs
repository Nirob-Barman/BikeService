using BikeService.Application.DTOs.PromoCode;
using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.PromoCodes.Queries.GetPromoCodeById;

public record GetPromoCodeByIdQuery(int Id) : IRequest<Result<PromoCodeDto>>;
