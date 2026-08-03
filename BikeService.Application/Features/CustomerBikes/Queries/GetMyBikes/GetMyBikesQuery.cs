using BikeService.Application.DTOs.CustomerBike;
using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.CustomerBikes.Queries.GetMyBikes;

public record GetMyBikesQuery : IRequest<Result<List<CustomerBikeDto>>>;
