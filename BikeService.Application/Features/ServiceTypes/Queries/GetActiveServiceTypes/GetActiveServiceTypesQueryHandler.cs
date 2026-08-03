using BikeService.Application.DTOs.ServiceType;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Mappers;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;
using MediatR;

namespace BikeService.Application.Features.ServiceTypes.Queries.GetActiveServiceTypes;

public class GetActiveServiceTypesQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetActiveServiceTypesQuery, Result<List<ServiceTypeDto>>>
{
    public async Task<Result<List<ServiceTypeDto>>> Handle(GetActiveServiceTypesQuery request, CancellationToken cancellationToken)
    {
        var items = await unitOfWork.Repository<ServiceType>()
            .GetAllAsync<ServiceTypeDto>(e => e.IsActive, e => ServiceTypeMapper.ToDto(e));
        return Result<List<ServiceTypeDto>>.Ok(items.ToList());
    }
}
