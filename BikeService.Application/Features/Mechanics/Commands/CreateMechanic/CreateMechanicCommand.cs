using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.Mechanics.Commands.CreateMechanic;

public record CreateMechanicCommand(
    string FullName,
    string? Specialty,
    bool IsAvailable,
    string? Email,
    string? Password) : IRequest<Result<int>>;
