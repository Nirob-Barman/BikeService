using BikeService.Application.Interfaces.Services;
using BikeService.Domain.Constants;
using BikeService.Web.ViewModels.Mappers;
using BikeService.Web.ViewModels.PromoCode;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BikeService.Web.Controllers.Admin
{
    [Authorize(Roles = AppRoles.Admin)]
    [Route("Admin/[controller]/[action]/{id?}")]
    public class PromoCodeController : Controller
    {
        private readonly IPromoCodeService _promoCodeService;

        public PromoCodeController(IPromoCodeService promoCodeService)
        {
            _promoCodeService = promoCodeService;
        }

        public async Task<IActionResult> Index()
        {
            var result = await _promoCodeService.GetAllAsync();
            if (!result.Success)
            {
                TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Failed to load promo codes.";
                return View(new List<BikeService.Application.DTOs.PromoCode.PromoCodeDto>());
            }
            return View(result.Data);
        }

        public IActionResult Create() => View(new PromoCodeFormViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PromoCodeFormViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var result = await _promoCodeService.CreateAsync(PromoCodeViewModelMapper.ToDto(vm));
            if (!result.Success)
            {
                if (result.FieldErrors != null)
                    foreach (var fe in result.FieldErrors)
                        ModelState.AddModelError(fe.Key, fe.Value);
                else
                    TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Failed to create promo code.";
                return View(vm);
            }

            TempData["Success"] = "Promo code created successfully.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var result = await _promoCodeService.GetByIdAsync(id);
            if (!result.Success)
            {
                TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Promo code not found.";
                return RedirectToAction(nameof(Index));
            }
            return View(PromoCodeViewModelMapper.ToViewModel(result.Data!));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PromoCodeFormViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var result = await _promoCodeService.UpdateAsync(id, PromoCodeViewModelMapper.ToDto(vm));
            if (!result.Success)
            {
                if (result.FieldErrors != null)
                    foreach (var fe in result.FieldErrors)
                        ModelState.AddModelError(fe.Key, fe.Value);
                else
                    TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Failed to update promo code.";
                return View(vm);
            }

            TempData["Success"] = "Promo code updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Toggle(int id)
        {
            var result = await _promoCodeService.ToggleActiveAsync(id);
            if (!result.Success)
                TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Failed to toggle promo code.";
            else
                TempData["Success"] = "Promo code status updated.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _promoCodeService.DeleteAsync(id);
            if (!result.Success)
                TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Failed to delete promo code.";
            else
                TempData["Success"] = "Promo code deleted successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}
