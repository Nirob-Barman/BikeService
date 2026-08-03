using BikeService.Application.Interfaces;
using BikeService.Application.Interfaces.FileStorage;
using BikeService.Application.Interfaces.Identity;
using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.Account.Commands.UploadProfilePhoto;

public class UploadProfilePhotoCommandHandler(
    IUserManager userManager,
    IUserContextService userContextService,
    IFileStorage fileStorage) : IRequestHandler<UploadProfilePhotoCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(UploadProfilePhotoCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userContextService.UserId!);
        if (user == null)
            return Result<bool>.Fail("User not found.");

        if (!string.IsNullOrEmpty(user.ProfileImageUrl))
            await fileStorage.DeleteFileAsync(user.ProfileImageUrl);

        var url = await fileStorage.UploadFileAsync(request.PhotoStream, request.FileName, "profiles");
        user.ProfileImageUrl = url;

        var (succeeded, errors) = await userManager.UpdateAsync(user);
        if (!succeeded)
            return Result<bool>.Fail(errors.FirstOrDefault() ?? "Failed to save photo.");

        return Result<bool>.Ok(true, "Profile photo updated.");
    }
}
