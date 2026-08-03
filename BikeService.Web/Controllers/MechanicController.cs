using BikeService.Application.DTOs.LeaveRequest;
using BikeService.Application.DTOs.ServiceTicket;
using BikeService.Application.Features.LeaveRequests.Commands.CancelLeaveRequest;
using BikeService.Application.Features.LeaveRequests.Commands.SubmitLeaveRequest;
using BikeService.Application.Features.LeaveRequests.Queries.GetMyLeaveRequests;
using BikeService.Application.Features.Payroll.Queries.GetMyPayroll;
using BikeService.Application.Features.ServiceTickets.Commands.AddServiceTicketItem;
using BikeService.Application.Features.ServiceTickets.Commands.RemoveServiceTicketItem;
using BikeService.Application.Features.ServiceTickets.Commands.UpdateServiceTicketDiagnosis;
using BikeService.Application.Features.ServiceTickets.Commands.UpdateServiceTicketStatus;
using BikeService.Application.Features.Parts.Queries.GetParts;
using BikeService.Application.Features.ServiceTickets.Queries.GetAssignedServiceTickets;
using BikeService.Application.Features.ServiceTickets.Queries.GetAssignedServiceTicketById;
using BikeService.Application.Features.ServiceTypes.Queries.GetActiveServiceTypes;
using BikeService.Application.Features.TicketNotes.Commands.AddTicketNote;
using BikeService.Application.Features.TicketNotes.Queries.GetTicketNotes;
using BikeService.Domain.Enums;
using BikeService.Web.ViewModels.LeaveRequest;
using BikeService.Web.ViewModels.Mappers;
using BikeService.Web.ViewModels.Mechanic;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BikeService.Application.DTOs.Payroll;

namespace BikeService.Web.Controllers
{
    [Authorize(Roles = "Mechanic")]
    public class MechanicController : Controller
    {
        private readonly IMediator _mediator;

        public MechanicController(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<IActionResult> Index()
        {
            var ticketsResult = await _mediator.Send(new GetAssignedServiceTicketsQuery());
            var leaveResult   = await _mediator.Send(new GetMyLeaveRequestsQuery());

            var active = ticketsResult.Success
                ? ticketsResult.Data!
                    .Where(t => t.Status != ServiceTicketStatus.Delivered && t.Status != ServiceTicketStatus.Cancelled)
                    .OrderBy(t => t.CreatedAt)
                    .ToList()
                : new List<ServiceTicketDto>();

            var leave = leaveResult.Success
                ? leaveResult.Data!.Take(5).ToList()
                : new List<LeaveRequestDto>();

            return View(new MechanicDashboardViewModel
            {
                ActiveTickets = active,
                RecentLeave   = leave,
            });
        }

        public async Task<IActionResult> Tickets()
        {
            var result = await _mediator.Send(new GetAssignedServiceTicketsQuery());
            if (!result.Success)
            {
                TempData["Error"] = result.Errors?.FirstOrDefault();
                return View(new List<ServiceTicketDto>());
            }

            var active = result.Data!
                .Where(t => t.Status != ServiceTicketStatus.Delivered && t.Status != ServiceTicketStatus.Cancelled)
                .OrderBy(t => t.CreatedAt)
                .ToList();

            return View(active);
        }

        public async Task<IActionResult> Detail(int id)
        {
            var result = await _mediator.Send(new GetAssignedServiceTicketByIdQuery(id));
            if (!result.Success)
            {
                TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Ticket not found.";
                return RedirectToAction(nameof(Index));
            }

            var ticket = result.Data!;

            var serviceTypes = await _mediator.Send(new GetActiveServiceTypesQuery());
            var parts = await _mediator.Send(new GetPartsQuery());
            var notes = await _mediator.Send(new GetTicketNotesQuery(id));

            return View(new MechanicTicketDetailViewModel
            {
                Ticket = ticket,
                ServiceTypes = serviceTypes.Data ?? new(),
                Parts = parts.Data ?? new(),
                Notes = notes.Data ?? new()
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddNote(int id, string message)
        {
            var result = await _mediator.Send(new AddTicketNoteCommand(id, message));

            if (!result.Success)
                TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Failed to add note.";

            return RedirectToAction(nameof(Detail), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdvanceStatus(int id, ServiceTicketStatus newStatus)
        {
            var result = await _mediator.Send(new UpdateServiceTicketStatusCommand(id, newStatus));
            if (!result.Success)
                TempData["Error"] = result.Errors?.FirstOrDefault();
            else
                TempData["Success"] = $"Status updated to {newStatus}.";

            return RedirectToAction(nameof(Detail), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateDiagnosis(int id, string? diagnosisNotes, DateTime? estimatedCompletion)
        {
            var result = await _mediator.Send(new UpdateServiceTicketDiagnosisCommand(id, diagnosisNotes, estimatedCompletion));
            if (!result.Success)
                TempData["Error"] = result.Errors?.FirstOrDefault();
            else
                TempData["Success"] = "Diagnosis notes saved.";

            return RedirectToAction(nameof(Detail), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddItem(int id, ServiceTicketItemFormDto dto)
        {
            var result = await _mediator.Send(new AddServiceTicketItemCommand(id, dto.ServiceTypeId, dto.PartId, dto.Quantity, dto.UnitPrice));
            if (!result.Success)
                TempData["Error"] = result.FieldErrors?.Values.FirstOrDefault() ?? result.Errors?.FirstOrDefault();
            else
                TempData["Success"] = "Item added.";

            return RedirectToAction(nameof(Detail), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveItem(int itemId, int ticketId)
        {
            var result = await _mediator.Send(new RemoveServiceTicketItemCommand(itemId));
            if (!result.Success)
                TempData["Error"] = result.Errors?.FirstOrDefault();

            return RedirectToAction(nameof(Detail), new { id = ticketId });
        }

        // ── Leave Requests ──────────────────────────────────────────────────────

        // GET: /Mechanic/Leave
        public async Task<IActionResult> Leave()
        {
            var result = await _mediator.Send(new GetMyLeaveRequestsQuery());
            if (!result.Success)
            {
                TempData["Error"] = result.Errors?.FirstOrDefault();
                return View(new List<LeaveRequestDto>());
            }
            return View(result.Data);
        }

        // GET: /Mechanic/LeaveCreate
        public IActionResult LeaveCreate()
        {
            return View(new LeaveRequestFormViewModel());
        }

        // POST: /Mechanic/LeaveCreate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LeaveCreate(LeaveRequestFormViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var dto = LeaveRequestViewModelMapper.ToDto(vm);
            var result = await _mediator.Send(new SubmitLeaveRequestCommand(dto.FromDate, dto.ToDate, dto.Type, dto.Reason));

            if (!result.Success)
            {
                if (result.FieldErrors != null)
                    foreach (var fe in result.FieldErrors)
                        ModelState.AddModelError(fe.Key, fe.Value);
                else
                    TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Failed to submit leave request.";
                return View(vm);
            }

            TempData["Success"] = result.Message;
            return RedirectToAction(nameof(Leave));
        }

        // POST: /Mechanic/LeaveCancel/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LeaveCancel(int id)
        {
            var result = await _mediator.Send(new CancelLeaveRequestCommand(id));
            if (!result.Success)
                TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Failed to cancel leave request.";
            else
                TempData["Success"] = result.Message;

            return RedirectToAction(nameof(Leave));
        }

        // GET: /Mechanic/Payroll
        public async Task<IActionResult> Payroll()
        {
            var result = await _mediator.Send(new GetMyPayrollQuery());
            if (!result.Success)
            {
                TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Failed to load payroll records.";
                return View(new List<PayrollRecordDto>());
            }
            return View(result.Data);
        }
    }
}
