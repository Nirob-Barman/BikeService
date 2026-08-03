using BikeService.Application.DTOs.PromoCode;
using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.PromoCodes.Queries.GetActivePromoCodes;

public record GetActivePromoCodesQuery : IRequest<Result<List<PromoCodeDto>>>;
