using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.CustomerBikes.Commands.UpdateCustomerBike;

public record UpdateCustomerBikeCommand(
    int Id,
    string Make,
    string Model,
    int Year,
    string? RegistrationNo,
    string? ImageUrl) : IRequest<Result<bool>>;
