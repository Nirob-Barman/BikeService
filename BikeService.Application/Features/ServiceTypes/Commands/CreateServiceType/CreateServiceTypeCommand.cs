using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.ServiceTypes.Commands.CreateServiceType;

public record CreateServiceTypeCommand(
    string Name,
    string? Description,
    decimal BasePrice,
    double EstimatedHours,
    bool IsActive) : IRequest<Result<int>>;
