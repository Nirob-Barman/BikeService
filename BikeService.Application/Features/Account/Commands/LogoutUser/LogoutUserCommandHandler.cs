using BikeService.Application.Interfaces.Identity;
using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.Account.Commands.LogoutUser;

public class LogoutUserCommandHandler(ISignInManager signInManager) : IRequestHandler<LogoutUserCommand, Result<string>>
{
    public async Task<Result<string>> Handle(LogoutUserCommand request, CancellationToken cancellationToken)
    {
        await signInManager.SignOutAsync();
        return Result<string>.Ok("Success", "Logout successful");
    }
}
