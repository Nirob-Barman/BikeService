using BikeService.Application.DTOs.Part;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Mappers;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;
using MediatR;

namespace BikeService.Application.Features.Parts.Queries.GetPartById;

public class GetPartByIdQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetPartByIdQuery, Result<PartDto>>
{
    public async Task<Result<PartDto>> Handle(GetPartByIdQuery request, CancellationToken cancellationToken)
    {
        var part = await unitOfWork.Repository<Part>().GetByIdAsync(request.Id);
        if (part is null)
            return Result<PartDto>.Fail("Part not found.");

        return Result<PartDto>.Ok(PartMapper.ToDto(part));
    }
}
