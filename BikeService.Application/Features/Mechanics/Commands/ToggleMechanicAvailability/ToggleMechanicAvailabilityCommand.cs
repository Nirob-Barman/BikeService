using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.Mechanics.Commands.ToggleMechanicAvailability;

public record ToggleMechanicAvailabilityCommand(int Id) : IRequest<Result<bool>>;
