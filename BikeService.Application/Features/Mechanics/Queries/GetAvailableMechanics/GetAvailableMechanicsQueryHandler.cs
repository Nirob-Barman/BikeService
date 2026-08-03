using BikeService.Application.DTOs.Mechanic;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Mappers;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;
using MediatR;

namespace BikeService.Application.Features.Mechanics.Queries.GetAvailableMechanics;

public class GetAvailableMechanicsQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetAvailableMechanicsQuery, Result<List<MechanicDto>>>
{
    public async Task<Result<List<MechanicDto>>> Handle(GetAvailableMechanicsQuery request, CancellationToken cancellationToken)
    {
        var items = await unitOfWork.Repository<Mechanic>()
            .GetAllAsync<MechanicDto>(e => e.IsAvailable, e => MechanicMapper.ToDto(e));
        return Result<List<MechanicDto>>.Ok(items.ToList());
    }
}
