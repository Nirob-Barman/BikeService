using BikeService.Application.Interfaces.Services;
using BikeService.Domain.Constants;
using BikeService.Web.ViewModels.Mappers;
using BikeService.Web.ViewModels.Mechanic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BikeService.Web.Controllers.Admin
{
    [Authorize(Roles = AppRoles.Admin)]
    [Route("Admin/Mechanic/[action]/{id?}")]
    public class AdminMechanicController : Controller
    {
        private readonly IMechanicService _mechanicService;

        public AdminMechanicController(IMechanicService mechanicService)
        {
            _mechanicService = mechanicService;
        }

        public async Task<IActionResult> Index()
        {
            var result = await _mechanicService.GetAllAsync();
            if (!result.Success)
            {
                TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Failed to load mechanics.";
                return View(new List<BikeService.Application.DTOs.Mechanic.MechanicDto>());
            }
            return View(result.Data);
        }

        public IActionResult Create() => View(new MechanicFormViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MechanicFormViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var result = await _mechanicService.CreateAsync(MechanicViewModelMapper.ToDto(vm));
            if (!result.Success)
            {
                if (result.FieldErrors != null)
                    foreach (var fe in result.FieldErrors)
                        ModelState.AddModelError(fe.Key, fe.Value);
                else
                    TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Failed to create mechanic.";
                return View(vm);
            }

            TempData["Success"] = "Mechanic created successfully.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var result = await _mechanicService.GetByIdAsync(id);
            if (!result.Success)
            {
                TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Mechanic not found.";
                return RedirectToAction(nameof(Index));
            }
            return View(MechanicViewModelMapper.ToViewModel(result.Data!));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MechanicFormViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var result = await _mechanicService.UpdateAsync(id, MechanicViewModelMapper.ToDto(vm));
            if (!result.Success)
            {
                if (result.FieldErrors != null)
                    foreach (var fe in result.FieldErrors)
                        ModelState.AddModelError(fe.Key, fe.Value);
                else
                    TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Failed to update mechanic.";
                return View(vm);
            }

            TempData["Success"] = "Mechanic updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Toggle(int id)
        {
            var result = await _mechanicService.ToggleAvailabilityAsync(id);
            if (!result.Success)
                TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Failed to toggle mechanic availability.";
            else
                TempData["Success"] = "Mechanic availability updated.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateLogin(int id, string email, string password, string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                TempData["LoginError"] = "Email and password are required.";
                return RedirectToAction(nameof(Edit), new { id });
            }

            if (password != confirmPassword)
            {
                TempData["LoginError"] = "Passwords do not match.";
                return RedirectToAction(nameof(Edit), new { id });
            }

            var result = await _mechanicService.CreateLoginAsync(id, email.Trim(), password);
            if (!result.Success)
                TempData["LoginError"] = result.Errors?.FirstOrDefault() ?? "Failed to create login.";
            else
                TempData["Success"] = "Login account created. The mechanic can now sign in.";

            return RedirectToAction(nameof(Edit), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleLogin(int id)
        {
            var result = await _mechanicService.ToggleLoginAsync(id);
            if (!result.Success)
                TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Failed to toggle login.";
            else
                TempData["Success"] = result.Message;

            return RedirectToAction(nameof(Edit), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _mechanicService.DeleteAsync(id);
            if (!result.Success)
                TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Failed to delete mechanic.";
            else
                TempData["Success"] = "Mechanic deleted successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}
