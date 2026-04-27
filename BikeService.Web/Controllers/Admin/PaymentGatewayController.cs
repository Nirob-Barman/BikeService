using BikeService.Application.DTOs.PaymentGateway;
using BikeService.Application.Interfaces.Services;
using BikeService.Domain.Constants;
using BikeService.Web.ViewModels.Mappers;
using BikeService.Web.ViewModels.PaymentGateway;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BikeService.Web.Controllers.Admin
{
    [Authorize(Roles = AppRoles.Admin)]
    [Route("Admin/[controller]")]
    public class PaymentGatewayController : Controller
    {
        private readonly IPaymentGatewayService _gatewayService;

        public PaymentGatewayController(IPaymentGatewayService gatewayService)
        {
            _gatewayService = gatewayService;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var result = await _gatewayService.GetAllAsync();
            if (!result.Success)
            {
                TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Failed to load payment gateways.";
                return View(new List<PaymentGatewayDto>());
            }
            return View(result.Data);
        }

        [HttpGet("Create")]
        public IActionResult Create() => View(new PaymentGatewayFormViewModel());

        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PaymentGatewayFormViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var result = await _gatewayService.CreateAsync(PaymentGatewayViewModelMapper.ToDto(vm));
            if (!result.Success)
            {
                TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Failed to create gateway.";
                return View(vm);
            }

            TempData["Success"] = "Payment gateway created.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var result = await _gatewayService.GetByIdAsync(id);
            if (!result.Success)
            {
                TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Gateway not found.";
                return RedirectToAction(nameof(Index));
            }

            var dto = result.Data!;
            var vm = new PaymentGatewayFormViewModel
            {
                Id        = dto.Id,
                Slug      = dto.Slug,
                Name      = dto.Name,
                IsActive  = dto.IsActive,
                IsSandbox = dto.IsSandbox,
            };

            var configResult = await _gatewayService.GetDecryptedConfigAsync(dto.Id);
            if (configResult.Success && !string.IsNullOrWhiteSpace(configResult.Data))
                PaymentGatewayViewModelMapper.PopulateFields(vm, configResult.Data);

            return View(vm);
        }

        [HttpPost("Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PaymentGatewayFormViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var result = await _gatewayService.UpdateAsync(id, PaymentGatewayViewModelMapper.ToDto(vm));
            if (!result.Success)
            {
                TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Failed to update gateway.";
                return View(vm);
            }

            TempData["Success"] = "Payment gateway updated.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("Toggle/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Toggle(int id)
        {
            var result = await _gatewayService.ToggleActiveAsync(id);
            if (!result.Success)
                TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Failed to toggle gateway.";
            else
                TempData["Success"] = "Gateway status toggled.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost("Delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _gatewayService.DeleteAsync(id);
            if (!result.Success)
                TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Failed to delete gateway.";
            else
                TempData["Success"] = "Payment gateway deleted.";

            return RedirectToAction(nameof(Index));
        }
    }
}
