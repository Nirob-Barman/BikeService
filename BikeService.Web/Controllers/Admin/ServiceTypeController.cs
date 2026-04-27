using BikeService.Application.Interfaces.Services;
using BikeService.Domain.Constants;
using BikeService.Web.ViewModels.Mappers;
using BikeService.Web.ViewModels.ServiceType;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BikeService.Web.Controllers.Admin
{
    [Authorize(Roles = AppRoles.Admin)]
    [Route("Admin/[controller]/[action]/{id?}")]
    public class ServiceTypeController : Controller
    {
        private readonly IServiceTypeService _serviceTypeService;

        public ServiceTypeController(IServiceTypeService serviceTypeService)
        {
            _serviceTypeService = serviceTypeService;
        }

        public async Task<IActionResult> Index()
        {
            var result = await _serviceTypeService.GetAllAsync();
            if (!result.Success)
            {
                TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Failed to load service types.";
                return View(new List<BikeService.Application.DTOs.ServiceType.ServiceTypeDto>());
            }
            return View(result.Data);
        }

        public IActionResult Create() => View(new ServiceTypeFormViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ServiceTypeFormViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var result = await _serviceTypeService.CreateAsync(ServiceTypeViewModelMapper.ToDto(vm));
            if (!result.Success)
            {
                if (result.FieldErrors != null)
                    foreach (var fe in result.FieldErrors)
                        ModelState.AddModelError(fe.Key, fe.Value);
                else
                    TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Failed to create service type.";
                return View(vm);
            }

            TempData["Success"] = "Service type created successfully.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var result = await _serviceTypeService.GetByIdAsync(id);
            if (!result.Success)
            {
                TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Service type not found.";
                return RedirectToAction(nameof(Index));
            }
            return View(ServiceTypeViewModelMapper.ToViewModel(result.Data!));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ServiceTypeFormViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var result = await _serviceTypeService.UpdateAsync(id, ServiceTypeViewModelMapper.ToDto(vm));
            if (!result.Success)
            {
                if (result.FieldErrors != null)
                    foreach (var fe in result.FieldErrors)
                        ModelState.AddModelError(fe.Key, fe.Value);
                else
                    TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Failed to update service type.";
                return View(vm);
            }

            TempData["Success"] = "Service type updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Toggle(int id)
        {
            var result = await _serviceTypeService.ToggleActiveAsync(id);
            if (!result.Success)
                TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Failed to toggle service type.";
            else
                TempData["Success"] = "Service type status updated.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _serviceTypeService.DeleteAsync(id);
            if (!result.Success)
                TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Failed to delete service type.";
            else
                TempData["Success"] = "Service type deleted successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}
