
using BikeService.Application.DTOs.Identity;
using BikeService.Application.Wrappers;

namespace BikeService.Application.Interfaces.Services
{
    public interface IUserService
    {
        Task<Result<string>> RegisterAsync(RegisterDto model);
        Task<Result<string>> LoginAsync(LoginDto model);
        Task<Result<string>> LogoutAsync();
        Task<Result<EditProfileDto>> GetProfileAsync();
        Task<Result<bool>> UpdateProfileAsync(EditProfileDto dto);
        Task<Result<bool>> ChangePasswordAsync(ChangePasswordDto dto);
        Task<Result<bool>> ForgotPasswordAsync(ForgotPasswordDto dto, string baseUrl);
        Task<Result<bool>> ResetPasswordAsync(ResetPasswordDto dto);
        Task<Result<bool>> UploadProfilePhotoAsync(Stream photoStream, string fileName);
    }
}
