using BikeService.Application.Features.Mechanics.Commands.CreateMechanic;
using BikeService.Application.Features.Mechanics.Commands.CreateMechanicLogin;
using BikeService.Application.Features.Mechanics.Commands.DeleteMechanic;
using BikeService.Application.Features.Mechanics.Commands.ToggleMechanicAvailability;
using BikeService.Application.Features.Mechanics.Commands.ToggleMechanicLogin;
using BikeService.Application.Features.Mechanics.Commands.UpdateMechanic;
using BikeService.Application.Features.Mechanics.Queries.GetMechanicById;
using BikeService.Application.Features.Mechanics.Queries.GetMechanics;
using BikeService.Domain.Constants;
using BikeService.Web.ViewModels.Mappers;
using BikeService.Web.ViewModels.Mechanic;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BikeService.Web.Controllers.Admin
{
    [Authorize(Roles = AppRoles.Admin)]
    [Route("Admin/Mechanic")]
    public class AdminMechanicController : Controller
    {
        private readonly IMediator _mediator;

        public AdminMechanicController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var result = await _mediator.Send(new GetMechanicsQuery());
            if (!result.Success)
            {
                TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Failed to load mechanics.";
                return View(new List<BikeService.Application.DTOs.Mechanic.MechanicDto>());
            }
            return View(result.Data);
        }

        [HttpGet("Create")]
        public IActionResult Create() => View(new MechanicFormViewModel());

        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MechanicFormViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var dto = MechanicViewModelMapper.ToDto(vm);
            var result = await _mediator.Send(new CreateMechanicCommand(
                dto.FullName, dto.Specialty, dto.IsAvailable, dto.Email, dto.Password));
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

        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var result = await _mediator.Send(new GetMechanicByIdQuery(id));
            if (!result.Success)
            {
                TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Mechanic not found.";
                return RedirectToAction(nameof(Index));
            }
            return View(MechanicViewModelMapper.ToViewModel(result.Data!));
        }

        [HttpPost("Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MechanicFormViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var dto = MechanicViewModelMapper.ToDto(vm);
            var result = await _mediator.Send(new UpdateMechanicCommand(id, dto.FullName, dto.Specialty, dto.IsAvailable));
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

        [HttpPost("Toggle/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Toggle(int id)
        {
            var result = await _mediator.Send(new ToggleMechanicAvailabilityCommand(id));
            if (!result.Success)
                TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Failed to toggle mechanic availability.";
            else
                TempData["Success"] = "Mechanic availability updated.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost("CreateLogin/{id}")]
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

            var result = await _mediator.Send(new CreateMechanicLoginCommand(id, email.Trim(), password));
            if (!result.Success)
                TempData["LoginError"] = result.Errors?.FirstOrDefault() ?? "Failed to create login.";
            else
                TempData["Success"] = "Login account created. The mechanic can now sign in.";

            return RedirectToAction(nameof(Edit), new { id });
        }

        [HttpPost("ToggleLogin/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleLogin(int id)
        {
            var result = await _mediator.Send(new ToggleMechanicLoginCommand(id));
            if (!result.Success)
                TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Failed to toggle login.";
            else
                TempData["Success"] = result.Message;

            return RedirectToAction(nameof(Edit), new { id });
        }

        [HttpPost("Delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _mediator.Send(new DeleteMechanicCommand(id));
            if (!result.Success)
                TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Failed to delete mechanic.";
            else
                TempData["Success"] = "Mechanic deleted successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}
