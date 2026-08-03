using BikeService.Application.DTOs.Mechanic;
using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.Mechanics.Queries.GetMechanicById;

public record GetMechanicByIdQuery(int Id) : IRequest<Result<MechanicDto>>;
