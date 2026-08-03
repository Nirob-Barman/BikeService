using BikeService.Application.DTOs.CustomerBike;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Mappers;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;
using MediatR;

namespace BikeService.Application.Features.CustomerBikes.Queries.GetCustomerBikes;

public class GetCustomerBikesQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetCustomerBikesQuery, Result<List<CustomerBikeDto>>>
{
    public async Task<Result<List<CustomerBikeDto>>> Handle(GetCustomerBikesQuery request, CancellationToken cancellationToken)
    {
        var bikes = await unitOfWork.Repository<CustomerBike>().GetAllAsync();
        var dtos = bikes.Select(CustomerBikeMapper.ToDto).ToList();
        return Result<List<CustomerBikeDto>>.Ok(dtos);
    }
}
