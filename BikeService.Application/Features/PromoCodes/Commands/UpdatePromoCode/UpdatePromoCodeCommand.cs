using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.PromoCodes.Commands.UpdatePromoCode;

public record UpdatePromoCodeCommand(
    int Id,
    string Code,
    decimal DiscountPercent,
    int MaxUsages,
    DateTime? ExpiresAt,
    bool IsActive) : IRequest<Result<bool>>;
