using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.ServiceTypes.Commands.UpdateServiceType;

public record UpdateServiceTypeCommand(
    int Id,
    string Name,
    string? Description,
    decimal BasePrice,
    double EstimatedHours,
    bool IsActive) : IRequest<Result<bool>>;
