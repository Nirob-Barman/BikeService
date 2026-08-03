using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.Account.Commands.RegisterUser;

public record RegisterUserCommand(
    string? FirstName,
    string? LastName,
    string? Email,
    string? Password,
    string? Address) : IRequest<Result<string>>;
