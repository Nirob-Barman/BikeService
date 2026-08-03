using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.Account.Commands.LogoutUser;

public record LogoutUserCommand : IRequest<Result<string>>;
