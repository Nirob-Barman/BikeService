using BikeService.Application.DTOs.Customer;
using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.Customers.Queries.GetCustomers;

public record GetCustomersQuery : IRequest<Result<List<CustomerDto>>>;
