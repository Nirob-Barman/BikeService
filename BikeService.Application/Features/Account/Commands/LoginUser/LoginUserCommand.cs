using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.Account.Commands.LoginUser;

public record LoginUserCommand(string? Email, string? Password, bool RememberMe) : IRequest<Result<string>>;
