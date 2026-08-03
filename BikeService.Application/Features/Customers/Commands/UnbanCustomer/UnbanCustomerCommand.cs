using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.Customers.Commands.UnbanCustomer;

public record UnbanCustomerCommand(string Id) : IRequest<Result<bool>>;
