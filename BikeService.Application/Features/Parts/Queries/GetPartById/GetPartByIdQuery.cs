using BikeService.Application.DTOs.Part;
using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.Parts.Queries.GetPartById;

public record GetPartByIdQuery(int Id) : IRequest<Result<PartDto>>;
