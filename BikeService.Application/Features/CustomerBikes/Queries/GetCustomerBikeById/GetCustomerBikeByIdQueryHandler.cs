using BikeService.Application.DTOs.CustomerBike;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Mappers;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;
using MediatR;

namespace BikeService.Application.Features.CustomerBikes.Queries.GetCustomerBikeById;

public class GetCustomerBikeByIdQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetCustomerBikeByIdQuery, Result<CustomerBikeDto>>
{
    public async Task<Result<CustomerBikeDto>> Handle(GetCustomerBikeByIdQuery request, CancellationToken cancellationToken)
    {
        var bike = await unitOfWork.Repository<CustomerBike>().GetByIdAsync(request.Id);
        if (bike is null)
            return Result<CustomerBikeDto>.Fail("Bike not found.");

        return Result<CustomerBikeDto>.Ok(CustomerBikeMapper.ToDto(bike));
    }
}
