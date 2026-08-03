using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.Mechanics.Commands.UpdateMechanic;

public record UpdateMechanicCommand(
    int Id,
    string FullName,
    string? Specialty,
    bool IsAvailable) : IRequest<Result<bool>>;
