using BikeService.Application.DTOs.Identity;
using BikeService.Web.ViewModels.Account;

namespace BikeService.Web.ViewModels.Mappers;

public static class AccountMapper
{
    public static RegisterDto ToDto(RegisterViewModel vm) =>
        new()
        {
            FirstName = vm.FirstName,
            LastName  = vm.LastName,
            Email     = vm.Email,
            Password  = vm.Password,
            Address   = vm.Address,
        };

    public static LoginDto ToDto(LoginViewModel vm) =>
        new()
        {
            Email      = vm.Email,
            Password   = vm.Password,
            RememberMe = vm.RememberMe,
        };

    public static EditProfileDto ToDto(ProfileViewModel vm) =>
        new()
        {
            FirstName = vm.FirstName,
            LastName  = vm.LastName,
            Address   = vm.Address,
        };

    public static ChangePasswordDto ToDto(ChangePasswordViewModel vm) =>
        new()
        {
            CurrentPassword = vm.CurrentPassword,
            NewPassword     = vm.NewPassword,
            ConfirmPassword = vm.ConfirmPassword,
        };

    public static ForgotPasswordDto ToDto(ForgotPasswordViewModel vm) =>
        new() { Email = vm.Email };

    public static ResetPasswordDto ToDto(ResetPasswordViewModel vm) =>
        new()
        {
            Email       = vm.Email,
            Token       = vm.Token,
            NewPassword = vm.NewPassword,
        };

    public static ProfileViewModel ToViewModel(EditProfileDto dto) =>
        new()
        {
            FirstName       = dto.FirstName,
            LastName        = dto.LastName,
            Address         = dto.Address,
            Email           = dto.Email,
            ProfileImageUrl = dto.ProfileImageUrl,
        };
}
