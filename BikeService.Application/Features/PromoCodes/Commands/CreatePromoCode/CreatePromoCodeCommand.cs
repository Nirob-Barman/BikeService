using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.PromoCodes.Commands.CreatePromoCode;

public record CreatePromoCodeCommand(
    string Code,
    decimal DiscountPercent,
    int MaxUsages,
    DateTime? ExpiresAt,
    bool IsActive) : IRequest<Result<int>>;
