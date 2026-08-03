using BikeService.Application.DTOs.CustomerBike;
using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.CustomerBikes.Queries.GetCustomerBikeById;

public record GetCustomerBikeByIdQuery(int Id) : IRequest<Result<CustomerBikeDto>>;
