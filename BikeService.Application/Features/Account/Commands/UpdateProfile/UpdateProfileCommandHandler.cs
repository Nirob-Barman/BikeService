using BikeService.Application.Interfaces;
using BikeService.Application.Interfaces.Identity;
using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.Account.Commands.UpdateProfile;

public class UpdateProfileCommandHandler(
    IUserManager userManager,
    IUserContextService userContextService) : IRequestHandler<UpdateProfileCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userContextService.UserId!);
        if (user == null)
            return Result<bool>.Fail("User not found.");

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.Address = request.Address;

        var (succeeded, errors) = await userManager.UpdateAsync(user);
        if (!succeeded)
            return Result<bool>.Fail(errors.FirstOrDefault() ?? "Profile update failed.");

        return Result<bool>.Ok(true, "Profile updated successfully.");
    }
}
