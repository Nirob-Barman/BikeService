using BikeService.Application.DTOs.Mechanic;
using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.Mechanics.Queries.GetAvailableMechanics;

public record GetAvailableMechanicsQuery : IRequest<Result<List<MechanicDto>>>;
