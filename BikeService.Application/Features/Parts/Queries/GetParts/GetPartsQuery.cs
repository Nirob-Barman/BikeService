using BikeService.Application.DTOs.Part;
using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.Parts.Queries.GetParts;

public record GetPartsQuery : IRequest<Result<List<PartDto>>>;
