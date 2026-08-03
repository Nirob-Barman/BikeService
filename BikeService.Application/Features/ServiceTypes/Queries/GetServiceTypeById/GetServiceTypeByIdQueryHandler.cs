using BikeService.Application.DTOs.ServiceType;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Mappers;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;
using MediatR;

namespace BikeService.Application.Features.ServiceTypes.Queries.GetServiceTypeById;

public class GetServiceTypeByIdQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetServiceTypeByIdQuery, Result<ServiceTypeDto>>
{
    public async Task<Result<ServiceTypeDto>> Handle(GetServiceTypeByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await unitOfWork.Repository<ServiceType>().GetByIdAsync(request.Id);
        if (entity == null)
            return Result<ServiceTypeDto>.Fail("Service type not found.");
        return Result<ServiceTypeDto>.Ok(ServiceTypeMapper.ToDto(entity));
    }
}
