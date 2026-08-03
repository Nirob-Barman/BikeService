using BikeService.Application.Interfaces.Identity;
using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.Account.Commands.LoginUser;

public class LoginUserCommandHandler(
    IUserManager userManager,
    ISignInManager signInManager) : IRequestHandler<LoginUserCommand, Result<string>>
{
    public async Task<Result<string>> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(request.Email!);
        if (user == null)
            return Result<string>.FailField(nameof(request.Email), "This email is not registered.");

        if (user.IsBanned)
            return Result<string>.Fail("Your account has been banned. Please contact support.");

        var isPasswordValid = await signInManager.CheckPasswordSignInAsync(user, request.Password!);
        if (!isPasswordValid)
            return Result<string>.FailField(nameof(request.Password), "Incorrect password.");

        await signInManager.SignInAsync(user, isPersistent: request.RememberMe);

        return Result<string>.Ok("Success", "Login successful");
    }
}
