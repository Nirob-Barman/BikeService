using BikeService.Application.DTOs.Mechanic;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Mappers;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;
using MediatR;

namespace BikeService.Application.Features.Mechanics.Queries.GetMechanics;

public class GetMechanicsQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetMechanicsQuery, Result<List<MechanicDto>>>
{
    public async Task<Result<List<MechanicDto>>> Handle(GetMechanicsQuery request, CancellationToken cancellationToken)
    {
        var items = await unitOfWork.Repository<Mechanic>()
            .GetAllAsync<MechanicDto>(e => MechanicMapper.ToDto(e));
        return Result<List<MechanicDto>>.Ok(items.ToList());
    }
}
