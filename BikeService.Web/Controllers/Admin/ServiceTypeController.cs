using BikeService.Application.Features.ServiceTypes.Commands.CreateServiceType;
using BikeService.Application.Features.ServiceTypes.Commands.DeleteServiceType;
using BikeService.Application.Features.ServiceTypes.Commands.ToggleServiceTypeActive;
using BikeService.Application.Features.ServiceTypes.Commands.UpdateServiceType;
using BikeService.Application.Features.ServiceTypes.Queries.GetServiceTypeById;
using BikeService.Application.Features.ServiceTypes.Queries.GetServiceTypes;
using BikeService.Domain.Constants;
using BikeService.Web.ViewModels.Mappers;
using BikeService.Web.ViewModels.ServiceType;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BikeService.Web.Controllers.Admin
{
    [Authorize(Roles = AppRoles.Admin)]
    [Route("Admin/[controller]")]
    public class ServiceTypeController : Controller
    {
        private readonly IMediator _mediator;

        public ServiceTypeController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var result = await _mediator.Send(new GetServiceTypesQuery());
            if (!result.Success)
            {
                TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Failed to load service types.";
                return View(new List<BikeService.Application.DTOs.ServiceType.ServiceTypeDto>());
            }
            return View(result.Data);
        }

        [HttpGet("Create")]
        public IActionResult Create() => View(new ServiceTypeFormViewModel());

        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ServiceTypeFormViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var dto = ServiceTypeViewModelMapper.ToDto(vm);
            var result = await _mediator.Send(new CreateServiceTypeCommand(
                dto.Name, dto.Description, dto.BasePrice, dto.EstimatedHours, dto.IsActive));
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

        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var result = await _mediator.Send(new GetServiceTypeByIdQuery(id));
            if (!result.Success)
            {
                TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Service type not found.";
                return RedirectToAction(nameof(Index));
            }
            return View(ServiceTypeViewModelMapper.ToViewModel(result.Data!));
        }

        [HttpPost("Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ServiceTypeFormViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var dto = ServiceTypeViewModelMapper.ToDto(vm);
            var result = await _mediator.Send(new UpdateServiceTypeCommand(
                id, dto.Name, dto.Description, dto.BasePrice, dto.EstimatedHours, dto.IsActive));
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

        [HttpPost("Toggle/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Toggle(int id)
        {
            var result = await _mediator.Send(new ToggleServiceTypeActiveCommand(id));
            if (!result.Success)
                TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Failed to toggle service type.";
            else
                TempData["Success"] = "Service type status updated.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost("Delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _mediator.Send(new DeleteServiceTypeCommand(id));
            if (!result.Success)
                TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Failed to delete service type.";
            else
                TempData["Success"] = "Service type deleted successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}
