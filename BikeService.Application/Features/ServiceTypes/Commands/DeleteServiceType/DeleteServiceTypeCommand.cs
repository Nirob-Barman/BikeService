using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.ServiceTypes.Commands.DeleteServiceType;

public record DeleteServiceTypeCommand(int Id) : IRequest<Result<bool>>;
