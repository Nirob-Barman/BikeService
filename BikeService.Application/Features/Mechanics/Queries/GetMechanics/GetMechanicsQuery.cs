using BikeService.Application.DTOs.Mechanic;
using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.Mechanics.Queries.GetMechanics;

public record GetMechanicsQuery : IRequest<Result<List<MechanicDto>>>;
