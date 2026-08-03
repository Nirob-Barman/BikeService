using BikeService.Application.DTOs.Customer;
using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.Customers.Queries.GetCustomerById;

public record GetCustomerByIdQuery(string Id) : IRequest<Result<CustomerDto>>;
