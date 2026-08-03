using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.Customers.Commands.BanCustomer;

public record BanCustomerCommand(string Id) : IRequest<Result<bool>>;
