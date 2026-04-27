using BikeService.Application.Interfaces.Services;
using BikeService.Domain.Constants;
using BikeService.Web.ViewModels.Inventory;
using BikeService.Web.ViewModels.Mappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BikeService.Web.Controllers.Admin
{
    [Authorize(Roles = AppRoles.Admin)]
    [Route("Admin/[controller]/[action]/{id?}")]
    public class InventoryController : Controller
    {
        private readonly IPartService _partService;
        private readonly IBulkImportService _bulkImportService;

        public InventoryController(IPartService partService, IBulkImportService bulkImportService)
        {
            _partService = partService;
            _bulkImportService = bulkImportService;
        }

        public async Task<IActionResult> Index()
        {
            var partsResult = await _partService.GetAllAsync();
            if (!partsResult.Success)
            {
                TempData["Error"] = partsResult.Errors?.FirstOrDefault() ?? "Failed to load parts.";
                return View(new List<BikeService.Application.DTOs.Part.PartDto>());
            }

            var alertsResult = await _partService.GetStockAlertsAsync(unresolvedOnly: true);
            ViewBag.UnresolvedAlertCount = alertsResult.Success ? alertsResult.Data?.Count ?? 0 : 0;

            return View(partsResult.Data);
        }

        public IActionResult Create() => View(new PartFormViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PartFormViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var dto = PartViewModelMapper.ToDto(vm);
            var result = await _partService.CreateAsync(dto);

            if (!result.Success)
            {
                if (result.FieldErrors != null)
                    foreach (var fe in result.FieldErrors)
                        ModelState.AddModelError(fe.Key, fe.Value);
                else
                    TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Failed to create part.";
                return View(vm);
            }

            TempData["Success"] = "Part created successfully.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var result = await _partService.GetByIdAsync(id);
            if (!result.Success)
            {
                TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Part not found.";
                return RedirectToAction(nameof(Index));
            }
            return View(PartViewModelMapper.ToViewModel(result.Data!));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PartFormViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var result = await _partService.UpdateAsync(id, PartViewModelMapper.ToDto(vm));

            if (!result.Success)
            {
                if (result.FieldErrors != null)
                    foreach (var fe in result.FieldErrors)
                        ModelState.AddModelError(fe.Key, fe.Value);
                else
                    TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Failed to update part.";
                return View(vm);
            }

            TempData["Success"] = "Part updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _partService.DeleteAsync(id);
            if (!result.Success)
                TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Failed to delete part.";
            else
                TempData["Success"] = "Part deleted successfully.";

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> StockAlerts()
        {
            var result = await _partService.GetStockAlertsAsync(unresolvedOnly: true);
            if (!result.Success)
            {
                TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Failed to load stock alerts.";
                return View(new List<BikeService.Application.DTOs.Part.PartStockAlertDto>());
            }
            return View(result.Data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResolveAlert(int alertId)
        {
            var result = await _partService.ResolveStockAlertAsync(alertId);
            if (!result.Success)
                TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Failed to resolve alert.";
            else
                TempData["Success"] = "Stock alert resolved.";

            return RedirectToAction(nameof(StockAlerts));
        }

        public IActionResult BulkImport() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkImport(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "Please select a CSV file to upload.";
                return View();
            }

            if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            {
                TempData["Error"] = "Only .csv files are accepted.";
                return View();
            }

            using var stream = file.OpenReadStream();
            var result = await _bulkImportService.ImportPartsAsync(stream, file.FileName);

            if (!result.Success)
            {
                TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Import failed.";
                return View();
            }

            ViewBag.ImportResult = result.Data;
            return View();
        }
    }
}
