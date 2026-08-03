using BikeService.Application.Features.Account.Commands.ChangePassword;
using BikeService.Application.Features.Account.Commands.ForgotPassword;
using BikeService.Application.Features.Account.Commands.LoginUser;
using BikeService.Application.Features.Account.Commands.LogoutUser;
using BikeService.Application.Features.Account.Commands.RegisterUser;
using BikeService.Application.Features.Account.Commands.ResetPassword;
using BikeService.Application.Features.Account.Commands.UpdateProfile;
using BikeService.Application.Features.Account.Commands.UploadProfilePhoto;
using BikeService.Application.Features.Account.Queries.GetProfile;
using BikeService.Application.Interfaces;
using BikeService.Web.ViewModels.Account;
using BikeService.Web.ViewModels.Mappers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BikeService.Web.Controllers;

public class AccountController : Controller
{
    private readonly IMediator _mediator;
    private readonly IUserContextService _userContextService;

    public AccountController(IMediator mediator, IUserContextService userContextService)
    {
        _mediator = mediator;
        _userContextService = userContextService;
    }

    // ── Login ────────────────────────────────────────────────────────────────

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToLocal(returnUrl);

        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
            return View(model);

        var dto = AccountMapper.ToDto(model);
        var result = await _mediator.Send(new LoginUserCommand(dto.Email, dto.Password, dto.RememberMe));

        if (!result.Success)
        {
            if (result.FieldErrors is not null)
                foreach (var (field, error) in result.FieldErrors)
                    ModelState.AddModelError(field, error);
            else
                ModelState.AddModelError(string.Empty, result.Errors?.FirstOrDefault() ?? "Login failed.");

            return View(model);
        }

        return RedirectToLocal(returnUrl);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _mediator.Send(new LogoutUserCommand());
        return RedirectToAction("Index", "Home");
    }

    // ── Register ─────────────────────────────────────────────────────────────

    [HttpGet]
    public IActionResult Register() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var dto = AccountMapper.ToDto(model);
        var result = await _mediator.Send(new RegisterUserCommand(dto.FirstName, dto.LastName, dto.Email, dto.Password, dto.Address));

        if (!result.Success)
        {
            if (result.FieldErrors is not null)
                foreach (var (field, error) in result.FieldErrors)
                    ModelState.AddModelError(field, error);
            else
                ModelState.AddModelError(string.Empty, result.Errors?.FirstOrDefault() ?? "Registration failed.");

            return View(model);
        }

        return RedirectToAction(nameof(Login));
    }

    // ── Profile ───────────────────────────────────────────────────────────────

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        var result = await _mediator.Send(new GetProfileQuery());
        if (!result.Success)
            return RedirectToAction(nameof(Login));

        return View(AccountMapper.ToViewModel(result.Data!));
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(ProfileViewModel vm)
    {
        if (!ModelState.IsValid)
            return View(vm);

        // Handle photo upload first (independent of profile save)
        if (vm.Photo != null && vm.Photo.Length > 0)
        {
            await using var stream = vm.Photo.OpenReadStream();
            var photoResult = await _mediator.Send(new UploadProfilePhotoCommand(stream, vm.Photo.FileName));
            if (!photoResult.Success)
                TempData["Error"] = photoResult.Errors?.FirstOrDefault() ?? "Photo upload failed.";
        }

        var dto = AccountMapper.ToDto(vm);
        var result = await _mediator.Send(new UpdateProfileCommand(dto.FirstName, dto.LastName, dto.Address));
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Errors?.FirstOrDefault() ?? "Update failed.");
            return View(vm);
        }

        TempData["Success"] = "Profile updated successfully.";
        return RedirectToAction(nameof(Profile));
    }

    // ── Change Password ───────────────────────────────────────────────────────

    [Authorize]
    [HttpGet]
    public IActionResult ChangePassword() => View();

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel vm)
    {
        if (!ModelState.IsValid)
            return View(vm);

        var dto = AccountMapper.ToDto(vm);
        var result = await _mediator.Send(new ChangePasswordCommand(dto.CurrentPassword, dto.NewPassword));
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Errors?.FirstOrDefault() ?? "Password change failed.");
            return View(vm);
        }

        TempData["Success"] = "Password changed successfully.";
        return RedirectToAction(nameof(Profile));
    }

    // ── Forgot Password ───────────────────────────────────────────────────────

    [HttpGet]
    public IActionResult ForgotPassword() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel vm)
    {
        if (!ModelState.IsValid)
            return View(vm);

        var dto = AccountMapper.ToDto(vm);
        await _mediator.Send(new ForgotPasswordCommand(dto.Email, _userContextService.GetBaseUrl()));
        return RedirectToAction(nameof(ForgotPasswordConfirmation));
    }

    [HttpGet]
    public IActionResult ForgotPasswordConfirmation() => View();

    // ── Reset Password ────────────────────────────────────────────────────────

    [HttpGet]
    public IActionResult ResetPassword(string? email, string? token)
    {
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
            return RedirectToAction(nameof(Login));

        return View(new ResetPasswordViewModel { Email = email, Token = token });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel vm)
    {
        if (!ModelState.IsValid)
            return View(vm);

        var dto = AccountMapper.ToDto(vm);
        var result = await _mediator.Send(new ResetPasswordCommand(dto.Email, dto.Token, dto.NewPassword));
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Errors?.FirstOrDefault() ?? "Password reset failed.");
            return View(vm);
        }

        return RedirectToAction(nameof(ResetPasswordConfirmation));
    }

    [HttpGet]
    public IActionResult ResetPasswordConfirmation() => View();

    // ── Helpers ───────────────────────────────────────────────────────────────

    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        if (User.IsInRole("Admin"))
            return RedirectToAction("Index", "Analytics");

        if (User.IsInRole("Mechanic"))
            return RedirectToAction("Index", "Mechanic");

        return RedirectToAction("Index", "Home");
    }
}
