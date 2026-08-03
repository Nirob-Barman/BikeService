using BikeService.Application.DTOs.CustomerBike;
using BikeService.Application.Interfaces;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Mappers;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;
using MediatR;

namespace BikeService.Application.Features.CustomerBikes.Queries.GetMyBikes;

public class GetMyBikesQueryHandler(
    IUnitOfWork unitOfWork,
    IUserContextService userContextService) : IRequestHandler<GetMyBikesQuery, Result<List<CustomerBikeDto>>>
{
    public async Task<Result<List<CustomerBikeDto>>> Handle(GetMyBikesQuery request, CancellationToken cancellationToken)
    {
        var userId = userContextService.UserId;
        if (string.IsNullOrEmpty(userId))
            return Result<List<CustomerBikeDto>>.Fail("User is not authenticated.");

        var bikes = await unitOfWork.Repository<CustomerBike>().Where(b => b.CustomerId == userId);
        var dtos = bikes.Select(CustomerBikeMapper.ToDto).ToList();
        return Result<List<CustomerBikeDto>>.Ok(dtos);
    }
}
