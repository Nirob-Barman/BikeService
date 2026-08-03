using BikeService.Application.Features.Parts.Commands.CreatePart;
using BikeService.Application.Features.Parts.Commands.DeletePart;
using BikeService.Application.Features.Parts.Commands.ImportParts;
using BikeService.Application.Features.Parts.Commands.ResolveStockAlert;
using BikeService.Application.Features.Parts.Commands.UpdatePart;
using BikeService.Application.Features.Parts.Queries.GetPartById;
using BikeService.Application.Features.Parts.Queries.GetParts;
using BikeService.Application.Features.Parts.Queries.GetStockAlerts;
using BikeService.Domain.Constants;
using BikeService.Web.ViewModels.Inventory;
using BikeService.Web.ViewModels.Mappers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BikeService.Web.Controllers.Admin
{
    [Authorize(Roles = AppRoles.Admin)]
    [Route("Admin/[controller]")]
    public class InventoryController : Controller
    {
        private readonly IMediator _mediator;

        public InventoryController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var partsResult = await _mediator.Send(new GetPartsQuery());
            if (!partsResult.Success)
            {
                TempData["Error"] = partsResult.Errors?.FirstOrDefault() ?? "Failed to load parts.";
                return View(new List<BikeService.Application.DTOs.Part.PartDto>());
            }

            var alertsResult = await _mediator.Send(new GetStockAlertsQuery(UnresolvedOnly: true));
            ViewBag.UnresolvedAlertCount = alertsResult.Success ? alertsResult.Data?.Count ?? 0 : 0;

            return View(partsResult.Data);
        }

        [HttpGet("Create")]
        public IActionResult Create() => View(new PartFormViewModel());

        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PartFormViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var dto = PartViewModelMapper.ToDto(vm);
            var result = await _mediator.Send(new CreatePartCommand(
                dto.Name, dto.SKU, dto.UnitPrice, dto.StockQuantity, dto.LowStockThreshold));

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

        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var result = await _mediator.Send(new GetPartByIdQuery(id));
            if (!result.Success)
            {
                TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Part not found.";
                return RedirectToAction(nameof(Index));
            }
            return View(PartViewModelMapper.ToViewModel(result.Data!));
        }

        [HttpPost("Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PartFormViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var dto = PartViewModelMapper.ToDto(vm);
            var result = await _mediator.Send(new UpdatePartCommand(
                id, dto.Name, dto.SKU, dto.UnitPrice, dto.StockQuantity, dto.LowStockThreshold));

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

        [HttpPost("Delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _mediator.Send(new DeletePartCommand(id));
            if (!result.Success)
                TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Failed to delete part.";
            else
                TempData["Success"] = "Part deleted successfully.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet("StockAlerts")]
        public async Task<IActionResult> StockAlerts()
        {
            var result = await _mediator.Send(new GetStockAlertsQuery(UnresolvedOnly: true));
            if (!result.Success)
            {
                TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Failed to load stock alerts.";
                return View(new List<BikeService.Application.DTOs.Part.PartStockAlertDto>());
            }
            return View(result.Data);
        }

        [HttpPost("ResolveAlert")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResolveAlert(int alertId)
        {
            var result = await _mediator.Send(new ResolveStockAlertCommand(alertId));
            if (!result.Success)
                TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Failed to resolve alert.";
            else
                TempData["Success"] = "Stock alert resolved.";

            return RedirectToAction(nameof(StockAlerts));
        }

        [HttpGet("BulkImport")]
        public IActionResult BulkImport() => View();

        [HttpPost("BulkImport")]
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
            var result = await _mediator.Send(new ImportPartsCommand(stream, file.FileName));

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
