using BikeService.Application.DTOs.Identity;
using BikeService.Application.Interfaces;
using BikeService.Application.Interfaces.FileStorage;
using BikeService.Application.Interfaces.Identity;
using BikeService.Application.Interfaces.Services;
using BikeService.Application.Wrappers;
using BikeService.Application.Mappers;
using BikeService.Domain.Constants;
using BikeService.Domain.Entities;

namespace BikeService.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserManager _userManager;
        private readonly ISignInManager _signInManager;
        private readonly IRoleManager _roleManager;
        private readonly IEmailService _emailService;
        private readonly IUserContextService _userContextService;
        private readonly IFileStorage _fileStorage;

        public UserService(
            IUserManager userManager,
            ISignInManager signInManager,
            IRoleManager roleManager,
            IEmailService emailService,
            IUserContextService userContextService,
            IFileStorage fileStorage)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _emailService = emailService;
            _userContextService = userContextService;
            _fileStorage = fileStorage;
        }

        public async Task<Result<string>> RegisterAsync(RegisterDto model)
        {
            var user = UserMapper.ToEntity(model);

            var (succeeded, userId, errors) = await _userManager.CreateAsync(user, model.Password!);

            if (!succeeded)
                return Result<string>.Fail(errors!, "Registration failed");

            var roleResult = await _userManager.AddToRoleAsync(new AppUser { Id = userId }, AppRoles.Customer);

            if (!roleResult.Succeeded)
            {
                await _userManager.RemoveFromRoleAsync(new AppUser { Id = userId }, AppRoles.Customer);
                return Result<string>.Fail("Failed to assign default role to user.");
            }

            try
            {
                var welcomeMessage = $"Hello {model.FirstName},<br>Welcome to BikeService! Thank you for registering.";
                await _emailService.SendEmailAsync("Welcome to BikeService", welcomeMessage, new List<string> { model.Email! });
            }
            catch { /* non-critical */ }

            return Result<string>.Ok(userId, "Registration successful");
        }

        public async Task<Result<string>> LoginAsync(LoginDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email!);
            if (user == null)
                return Result<string>.FailField(nameof(model.Email), "This email is not registered.");

            if (user.IsBanned)
                return Result<string>.Fail("Your account has been banned. Please contact support.");

            var isPasswordValid = await _signInManager.CheckPasswordSignInAsync(user, model.Password!);
            if (!isPasswordValid)
                return Result<string>.FailField(nameof(model.Password), "Incorrect password.");

            await _signInManager.SignInAsync(user, isPersistent: model.RememberMe);

            return Result<string>.Ok("Success", "Login successful");
        }

        public async Task<Result<string>> LogoutAsync()
        {
            await _signInManager.SignOutAsync();
            return Result<string>.Ok("Success", "Logout successful");
        }

        public async Task<Result<EditProfileDto>> GetProfileAsync()
        {
            var user = await _userManager.FindByIdAsync(_userContextService.UserId!);
            if (user == null)
                return Result<EditProfileDto>.Fail("User not found.");

            return Result<EditProfileDto>.Ok(new EditProfileDto
            {
                FirstName       = user.FirstName,
                LastName        = user.LastName,
                Address         = user.Address,
                Email           = user.Email,
                ProfileImageUrl = user.ProfileImageUrl,
            });
        }

        public async Task<Result<bool>> UpdateProfileAsync(EditProfileDto dto)
        {
            var user = await _userManager.FindByIdAsync(_userContextService.UserId!);
            if (user == null)
                return Result<bool>.Fail("User not found.");

            user.FirstName = dto.FirstName;
            user.LastName  = dto.LastName;
            user.Address   = dto.Address;

            var (succeeded, errors) = await _userManager.UpdateAsync(user);
            if (!succeeded)
                return Result<bool>.Fail(errors.FirstOrDefault() ?? "Profile update failed.");

            return Result<bool>.Ok(true, "Profile updated successfully.");
        }

        public async Task<Result<bool>> ChangePasswordAsync(ChangePasswordDto dto)
        {
            var (succeeded, errors) = await _userManager.ChangePasswordAsync(
                _userContextService.UserId!, dto.CurrentPassword!, dto.NewPassword!);

            if (!succeeded)
                return Result<bool>.Fail(errors.FirstOrDefault() ?? "Password change failed.");

            return Result<bool>.Ok(true, "Password changed successfully.");
        }

        public async Task<Result<bool>> ForgotPasswordAsync(ForgotPasswordDto dto, string baseUrl)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email!);

            // Always return Ok — never reveal whether an email is registered
            if (user == null)
                return Result<bool>.Ok(true);

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetLink = $"{baseUrl}/Account/ResetPassword" +
                            $"?email={Uri.EscapeDataString(dto.Email!)}" +
                            $"&token={Uri.EscapeDataString(token)}";

            var body = $@"<p>Hi {user.FirstName},</p>
<p>We received a request to reset your BikeService password.</p>
<p><a href='{resetLink}' style='padding:10px 20px;background:#0d6efd;color:#fff;text-decoration:none;border-radius:4px;'>Reset Password</a></p>
<p>If you did not request this, you can safely ignore this email. The link expires in 24 hours.</p>";

            try
            {
                await _emailService.SendEmailAsync(
                    "Reset Your BikeService Password", body, new List<string> { dto.Email! });
            }
            catch { /* non-critical */ }

            return Result<bool>.Ok(true);
        }

        public async Task<Result<bool>> UploadProfilePhotoAsync(Stream photoStream, string fileName)
        {
            var user = await _userManager.FindByIdAsync(_userContextService.UserId!);
            if (user == null)
                return Result<bool>.Fail("User not found.");

            // Delete existing photo
            if (!string.IsNullOrEmpty(user.ProfileImageUrl))
                await _fileStorage.DeleteFileAsync(user.ProfileImageUrl);

            var url = await _fileStorage.UploadFileAsync(photoStream, fileName, "profiles");
            user.ProfileImageUrl = url;

            var (succeeded, errors) = await _userManager.UpdateAsync(user);
            if (!succeeded)
                return Result<bool>.Fail(errors.FirstOrDefault() ?? "Failed to save photo.");

            return Result<bool>.Ok(true, "Profile photo updated.");
        }

        public async Task<Result<bool>> ResetPasswordAsync(ResetPasswordDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email!);
            if (user == null)
                return Result<bool>.Fail("Invalid password reset request.");

            var (succeeded, errors) = await _userManager.ResetPasswordAsync(user, dto.Token!, dto.NewPassword!);
            if (!succeeded)
                return Result<bool>.Fail(errors.FirstOrDefault() ?? "Password reset failed.");

            return Result<bool>.Ok(true, "Password reset successfully.");
        }
    }
}
