using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.CustomerBikes.Commands.DeleteCustomerBike;

public record DeleteCustomerBikeCommand(int Id) : IRequest<Result<bool>>;
