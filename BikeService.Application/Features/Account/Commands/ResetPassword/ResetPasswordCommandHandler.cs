using BikeService.Application.Interfaces.Identity;
using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.Account.Commands.ResetPassword;

public class ResetPasswordCommandHandler(IUserManager userManager) : IRequestHandler<ResetPasswordCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(request.Email!);
        if (user == null)
            return Result<bool>.Fail("Invalid password reset request.");

        var (succeeded, errors) = await userManager.ResetPasswordAsync(user, request.Token!, request.NewPassword!);
        if (!succeeded)
            return Result<bool>.Fail(errors.FirstOrDefault() ?? "Password reset failed.");

        return Result<bool>.Ok(true, "Password reset successfully.");
    }
}
