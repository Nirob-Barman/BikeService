using BikeService.Application.DTOs.CustomerBike;
using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.CustomerBikes.Queries.GetCustomerBikes;

public record GetCustomerBikesQuery : IRequest<Result<List<CustomerBikeDto>>>;
