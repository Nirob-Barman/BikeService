using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.ServiceTypes.Commands.ToggleServiceTypeActive;

public record ToggleServiceTypeActiveCommand(int Id) : IRequest<Result<bool>>;
