using BikeService.Application.Interfaces;
using BikeService.Application.Interfaces.Identity;
using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.Account.Commands.ForgotPassword;

public class ForgotPasswordCommandHandler(
    IUserManager userManager,
    IEmailService emailService) : IRequestHandler<ForgotPasswordCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(request.Email!);

        // Always return Ok — never reveal whether an email is registered
        if (user == null)
            return Result<bool>.Ok(true);

        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        var resetLink = $"{request.BaseUrl}/Account/ResetPassword" +
                        $"?email={Uri.EscapeDataString(request.Email!)}" +
                        $"&token={Uri.EscapeDataString(token)}";

        var body = $@"<p>Hi {user.FirstName},</p>
<p>We received a request to reset your BikeService password.</p>
<p><a href='{resetLink}' style='padding:10px 20px;background:#0d6efd;color:#fff;text-decoration:none;border-radius:4px;'>Reset Password</a></p>
<p>If you did not request this, you can safely ignore this email. The link expires in 24 hours.</p>";

        try
        {
            await emailService.SendEmailAsync(
                "Reset Your BikeService Password", body, new List<string> { request.Email! });
        }
        catch { /* non-critical */ }

        return Result<bool>.Ok(true);
    }
}
