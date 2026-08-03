using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.PromoCodes.Commands.TogglePromoCodeActive;

public record TogglePromoCodeActiveCommand(int Id) : IRequest<Result<bool>>;
