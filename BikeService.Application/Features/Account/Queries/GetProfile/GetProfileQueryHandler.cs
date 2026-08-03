using BikeService.Application.DTOs.Identity;
using BikeService.Application.Interfaces;
using BikeService.Application.Interfaces.Identity;
using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.Account.Queries.GetProfile;

public class GetProfileQueryHandler(
    IUserManager userManager,
    IUserContextService userContextService) : IRequestHandler<GetProfileQuery, Result<EditProfileDto>>
{
    public async Task<Result<EditProfileDto>> Handle(GetProfileQuery request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userContextService.UserId!);
        if (user == null)
            return Result<EditProfileDto>.Fail("User not found.");

        return Result<EditProfileDto>.Ok(new EditProfileDto
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Address = user.Address,
            Email = user.Email,
            ProfileImageUrl = user.ProfileImageUrl,
        });
    }
}
