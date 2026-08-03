using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.Mechanics.Commands.DeleteMechanic;

public record DeleteMechanicCommand(int Id) : IRequest<Result<bool>>;
