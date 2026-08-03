using BikeService.Application.DTOs.PaymentGateway;
using BikeService.Application.Features.PaymentGateways.Commands.CreatePaymentGateway;
using BikeService.Application.Features.PaymentGateways.Commands.DeletePaymentGateway;
using BikeService.Application.Features.PaymentGateways.Commands.TogglePaymentGatewayActive;
using BikeService.Application.Features.PaymentGateways.Commands.UpdatePaymentGateway;
using BikeService.Application.Features.PaymentGateways.Queries.GetDecryptedPaymentGatewayConfig;
using BikeService.Application.Features.PaymentGateways.Queries.GetPaymentGatewayById;
using BikeService.Application.Features.PaymentGateways.Queries.GetPaymentGateways;
using BikeService.Domain.Constants;
using BikeService.Web.ViewModels.Mappers;
using BikeService.Web.ViewModels.PaymentGateway;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BikeService.Web.Controllers.Admin
{
    [Authorize(Roles = AppRoles.Admin)]
    [Route("Admin/[controller]")]
    public class PaymentGatewayController : Controller
    {
        private readonly IMediator _mediator;

        public PaymentGatewayController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var result = await _mediator.Send(new GetPaymentGatewaysQuery());
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

            var dto = PaymentGatewayViewModelMapper.ToDto(vm);
            var result = await _mediator.Send(new CreatePaymentGatewayCommand(
                dto.Slug, dto.Name, dto.Config, dto.IsActive, dto.IsSandbox));
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
            var result = await _mediator.Send(new GetPaymentGatewayByIdQuery(id));
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

            var configResult = await _mediator.Send(new GetDecryptedPaymentGatewayConfigQuery(dto.Id));
            if (configResult.Success && !string.IsNullOrWhiteSpace(configResult.Data))
                PaymentGatewayViewModelMapper.PopulateFields(vm, configResult.Data);

            return View(vm);
        }

        [HttpPost("Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PaymentGatewayFormViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var dto = PaymentGatewayViewModelMapper.ToDto(vm);
            var result = await _mediator.Send(new UpdatePaymentGatewayCommand(
                id, dto.Slug, dto.Name, dto.Config, dto.IsActive, dto.IsSandbox));
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
            var result = await _mediator.Send(new TogglePaymentGatewayActiveCommand(id));
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
            var result = await _mediator.Send(new DeletePaymentGatewayCommand(id));
            if (!result.Success)
                TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Failed to delete gateway.";
            else
                TempData["Success"] = "Payment gateway deleted.";

            return RedirectToAction(nameof(Index));
        }
    }
}
