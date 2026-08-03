using BikeService.Application.DTOs.ServiceTicket;
using BikeService.Application.Features.CustomerBikes.Queries.GetCustomerBikes;
using BikeService.Application.Features.Invoices.Commands.GenerateInvoice;
using BikeService.Application.Features.Mechanics.Queries.GetAvailableMechanics;
using BikeService.Application.Features.Mechanics.Queries.GetMechanics;
using BikeService.Application.Features.Parts.Queries.GetParts;
using BikeService.Application.Features.ServiceTickets.Commands.AddServiceTicketItem;
using BikeService.Application.Features.ServiceTickets.Commands.AssignMechanicToTicket;
using BikeService.Application.Features.ServiceTickets.Commands.CancelServiceTicket;
using BikeService.Application.Features.ServiceTickets.Commands.CreateServiceTicket;
using BikeService.Application.Features.ServiceTickets.Commands.RemoveServiceTicketItem;
using BikeService.Application.Features.ServiceTickets.Commands.UpdateServiceTicketDiagnosis;
using BikeService.Application.Features.ServiceTickets.Commands.UpdateServiceTicketStatus;
using BikeService.Application.Features.ServiceTickets.Queries.GetServiceTicketById;
using BikeService.Application.Features.ServiceTickets.Queries.GetServiceTickets;
using BikeService.Application.Features.ServiceTypes.Queries.GetActiveServiceTypes;
using BikeService.Domain.Constants;
using BikeService.Domain.Enums;
using BikeService.Web.ViewModels.ServiceTicket;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BikeService.Web.Controllers.Admin
{
    [Authorize(Roles = AppRoles.Admin)]
    [Route("Admin/[controller]")]
    public class TicketController : Controller
    {
        private readonly IMediator _mediator;

        public TicketController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index(ServiceTicketStatus? status, int? mechanicId, DateTime? dateFrom, DateTime? dateTo)
        {
            var filter = new TicketFilterDto { Status = status, MechanicId = mechanicId, DateFrom = dateFrom, DateTo = dateTo };
            var result = await _mediator.Send(new GetServiceTicketsQuery(filter));
            if (!result.Success)
            {
                TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Failed to load tickets.";
                return View(new List<ServiceTicketDto>());
            }

            var mechanicsResult = await _mediator.Send(new GetAvailableMechanicsQuery());
            ViewBag.Mechanics        = mechanicsResult.Success ? mechanicsResult.Data : new List<Application.DTOs.Mechanic.MechanicDto>();
            ViewBag.StatusFilter     = status;
            ViewBag.MechanicIdFilter = mechanicId;
            ViewBag.DateFrom         = dateFrom?.ToString("yyyy-MM-dd");
            ViewBag.DateTo           = dateTo?.ToString("yyyy-MM-dd");

            return View(result.Data);
        }

        [HttpGet("Detail/{id}")]
        public async Task<IActionResult> Detail(int id)
        {
            var ticketResult = await _mediator.Send(new GetServiceTicketByIdQuery(id));
            if (!ticketResult.Success)
            {
                TempData["Error"] = ticketResult.Errors?.FirstOrDefault() ?? "Ticket not found.";
                return RedirectToAction(nameof(Index));
            }

            var mechanicsResult    = await _mediator.Send(new GetAvailableMechanicsQuery());
            var serviceTypesResult = await _mediator.Send(new GetActiveServiceTypesQuery());
            var partsResult        = await _mediator.Send(new GetPartsQuery());

            return View(new TicketDetailViewModel
            {
                Ticket             = ticketResult.Data!,
                AvailableMechanics = mechanicsResult.Success ? mechanicsResult.Data! : new(),
                ActiveServiceTypes = serviceTypesResult.Success ? serviceTypesResult.Data! : new(),
                AllParts           = partsResult.Success ? partsResult.Data! : new()
            });
        }

        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            await PopulateCreateDropdowns();
            return View(new WalkInTicketFormViewModel { EstimatedCompletionDate = DateTime.Today.AddDays(3) });
        }

        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(WalkInTicketFormViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                await PopulateCreateDropdowns();
                return View(vm);
            }

            var result = await _mediator.Send(new CreateServiceTicketCommand(
                vm.BikeId, vm.MechanicId, null, vm.DiagnosisNotes, vm.EstimatedCompletionDate));
            if (!result.Success)
            {
                TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Failed to create ticket.";
                await PopulateCreateDropdowns();
                return View(vm);
            }

            TempData["Success"] = "Walk-in ticket created successfully.";
            return RedirectToAction(nameof(Detail), new { id = result.Data });
        }

        [HttpPost("UpdateStatus/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, ServiceTicketStatus newStatus)
        {
            var result = await _mediator.Send(new UpdateServiceTicketStatusCommand(id, newStatus));
            if (!result.Success)
                TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Failed to update status.";
            else
                TempData["Success"] = "Ticket status updated.";

            return RedirectToAction(nameof(Detail), new { id });
        }

        [HttpPost("AssignMechanic/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignMechanic(int id, int mechanicId)
        {
            var result = await _mediator.Send(new AssignMechanicToTicketCommand(id, mechanicId));
            if (!result.Success)
                TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Failed to assign mechanic.";
            else
                TempData["Success"] = "Mechanic assigned successfully.";

            return RedirectToAction(nameof(Detail), new { id });
        }

        [HttpPost("UpdateDiagnosis/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateDiagnosis(int id, string? diagnosisNotes, DateTime? estimatedCompletionDate)
        {
            var result = await _mediator.Send(new UpdateServiceTicketDiagnosisCommand(id, diagnosisNotes, estimatedCompletionDate));
            if (!result.Success)
                TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Failed to update diagnosis.";
            else
                TempData["Success"] = "Diagnosis notes saved.";

            return RedirectToAction(nameof(Detail), new { id });
        }

        [HttpPost("AddItem")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddItem(int ticketId, ServiceTicketItemFormDto dto)
        {
            var result = await _mediator.Send(new AddServiceTicketItemCommand(ticketId, dto.ServiceTypeId, dto.PartId, dto.Quantity, dto.UnitPrice));
            if (!result.Success)
                TempData["Error"] = result.FieldErrors?.Values.FirstOrDefault() ?? result.Errors?.FirstOrDefault() ?? "Failed to add item.";
            else
                TempData["Success"] = "Item added to ticket.";

            return RedirectToAction(nameof(Detail), new { id = ticketId });
        }

        [HttpPost("RemoveItem")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveItem(int itemId, int ticketId)
        {
            var result = await _mediator.Send(new RemoveServiceTicketItemCommand(itemId));
            if (!result.Success)
                TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Failed to remove item.";
            else
                TempData["Success"] = "Item removed from ticket.";

            return RedirectToAction(nameof(Detail), new { id = ticketId });
        }

        [HttpPost("Cancel/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var result = await _mediator.Send(new CancelServiceTicketCommand(id));
            if (!result.Success)
                TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Failed to cancel ticket.";
            else
                TempData["Success"] = "Ticket cancelled.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost("GenerateInvoice/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateInvoice(int id)
        {
            var result = await _mediator.Send(new GenerateInvoiceCommand(id));
            if (!result.Success)
                TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Failed to generate invoice.";
            else
                TempData["Success"] = "Invoice generated successfully.";

            return RedirectToAction(nameof(Detail), new { id });
        }

        private async Task PopulateCreateDropdowns()
        {
            var bikes     = await _mediator.Send(new GetCustomerBikesQuery());
            var mechanics = await _mediator.Send(new GetMechanicsQuery());

            ViewBag.Bikes = new SelectList(
                (bikes.Data ?? []).Select(b => new
                {
                    b.Id,
                    Label = $"{b.Make} {b.Model} ({b.Year})" + (b.RegistrationNo != null ? $" — {b.RegistrationNo}" : "")
                }),
                "Id", "Label");

            ViewBag.Mechanics = new SelectList(mechanics.Data ?? [], "Id", "FullName");
        }
    }
}
