using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.Mechanics.Commands.ToggleMechanicLogin;

public record ToggleMechanicLoginCommand(int MechanicId) : IRequest<Result<bool>>;
