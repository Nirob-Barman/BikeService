using BikeService.Application.DTOs.Part;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Mappers;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;
using MediatR;

namespace BikeService.Application.Features.Parts.Queries.GetParts;

public class GetPartsQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetPartsQuery, Result<List<PartDto>>>
{
    public async Task<Result<List<PartDto>>> Handle(GetPartsQuery request, CancellationToken cancellationToken)
    {
        var parts = await unitOfWork.Repository<Part>().GetAllAsync();
        var dtos = parts.Select(PartMapper.ToDto).ToList();
        return Result<List<PartDto>>.Ok(dtos);
    }
}
