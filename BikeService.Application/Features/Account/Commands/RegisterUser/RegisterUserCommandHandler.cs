using BikeService.Application.Interfaces;
using BikeService.Application.Interfaces.Identity;
using BikeService.Application.Wrappers;
using BikeService.Domain.Constants;
using BikeService.Domain.Entities;
using MediatR;

namespace BikeService.Application.Features.Account.Commands.RegisterUser;

public class RegisterUserCommandHandler(
    IUserManager userManager,
    IEmailService emailService) : IRequestHandler<RegisterUserCommand, Result<string>>
{
    public async Task<Result<string>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var user = new AppUser
        {
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Address = request.Address,
        };

        var (succeeded, userId, errors) = await userManager.CreateAsync(user, request.Password!);
        if (!succeeded)
            return Result<string>.Fail(errors!, "Registration failed");

        var roleResult = await userManager.AddToRoleAsync(new AppUser { Id = userId }, AppRoles.Customer);
        if (!roleResult.Succeeded)
        {
            await userManager.RemoveFromRoleAsync(new AppUser { Id = userId }, AppRoles.Customer);
            return Result<string>.Fail("Failed to assign default role to user.");
        }

        try
        {
            var welcomeMessage = $"Hello {request.FirstName},<br>Welcome to BikeService! Thank you for registering.";
            await emailService.SendEmailAsync("Welcome to BikeService", welcomeMessage, new List<string> { request.Email! });
        }
        catch { /* non-critical */ }

        return Result<string>.Ok(userId, "Registration successful");
    }
}
