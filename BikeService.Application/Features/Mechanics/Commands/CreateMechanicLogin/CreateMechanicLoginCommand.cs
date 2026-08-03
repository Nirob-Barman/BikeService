using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.Mechanics.Commands.CreateMechanicLogin;

public record CreateMechanicLoginCommand(int MechanicId, string Email, string Password) : IRequest<Result<bool>>;
