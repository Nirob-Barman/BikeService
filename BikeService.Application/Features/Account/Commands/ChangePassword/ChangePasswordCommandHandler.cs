using BikeService.Application.Interfaces;
using BikeService.Application.Interfaces.Identity;
using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.Account.Commands.ChangePassword;

public class ChangePasswordCommandHandler(
    IUserManager userManager,
    IUserContextService userContextService) : IRequestHandler<ChangePasswordCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var (succeeded, errors) = await userManager.ChangePasswordAsync(
            userContextService.UserId!, request.CurrentPassword!, request.NewPassword!);

        if (!succeeded)
            return Result<bool>.Fail(errors.FirstOrDefault() ?? "Password change failed.");

        return Result<bool>.Ok(true, "Password changed successfully.");
    }
}
