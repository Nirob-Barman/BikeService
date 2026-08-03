using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.CustomerBikes.Commands.CreateCustomerBike;

public record CreateCustomerBikeCommand(
    string Make,
    string Model,
    int Year,
    string? RegistrationNo,
    string? ImageUrl) : IRequest<Result<int>>;
