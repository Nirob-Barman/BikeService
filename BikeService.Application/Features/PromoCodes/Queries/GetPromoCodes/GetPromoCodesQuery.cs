using BikeService.Application.DTOs.PromoCode;
using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.PromoCodes.Queries.GetPromoCodes;

public record GetPromoCodesQuery : IRequest<Result<List<PromoCodeDto>>>;
