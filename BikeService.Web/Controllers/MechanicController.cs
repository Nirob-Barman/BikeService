using BikeService.Application.DTOs.LeaveRequest;
using BikeService.Application.DTOs.ServiceTicket;
using BikeService.Application.DTOs.TicketNote;
using BikeService.Application.Interfaces.Services;
using BikeService.Domain.Enums;
using BikeService.Web.ViewModels.LeaveRequest;
using BikeService.Web.ViewModels.Mappers;
using BikeService.Web.ViewModels.Mechanic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using BikeService.Application.DTOs.Payroll;

namespace BikeService.Web.Controllers
{
    [Authorize(Roles = "Mechanic")]
    public class MechanicController : Controller
    {
        private readonly IServiceTicketService _ticketService;
        private readonly IServiceTypeService _serviceTypeService;
        private readonly IPartService _partService;
        private readonly ITicketNoteService _noteService;
        private readonly ILeaveRequestService _leaveService;
        private readonly IPayrollService _payrollService;

        public MechanicController(
            IServiceTicketService ticketService,
            IServiceTypeService serviceTypeService,
            IPartService partService,
            ITicketNoteService noteService,
            ILeaveRequestService leaveService,
            IPayrollService payrollService)
        {
            _ticketService = ticketService;
            _serviceTypeService = serviceTypeService;
            _partService = partService;
            _noteService = noteService;
            _leaveService = leaveService;
            _payrollService = payrollService;
        }

        public async Task<IActionResult> Index()
        {
            var ticketsResult = await _ticketService.GetAssignedTicketsAsync();
            var leaveResult   = await _leaveService.GetMyLeaveRequestsAsync();

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
            var result = await _ticketService.GetAssignedTicketsAsync();
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
            var result = await _ticketService.GetByIdAsync(id);
            if (!result.Success)
            {
                TempData["Error"] = "Ticket not found.";
                return RedirectToAction(nameof(Index));
            }

            // Ownership check — ticket must be assigned to this mechanic
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var ticket = result.Data!;
            // We can't directly compare MechanicId to userId; the service fetched the ticket with MechanicName
            // The assigned mechanic's UserId is on the Mechanic entity — we verify via GetAssignedTicketsAsync scope
            // For security, we re-check by verifying this ticket appears in the assigned list
            var assigned = await _ticketService.GetAssignedTicketsAsync();
            if (!assigned.Success || !assigned.Data!.Any(t => t.Id == id))
            {
                TempData["Error"] = "Access denied.";
                return RedirectToAction(nameof(Index));
            }

            var serviceTypes = await _serviceTypeService.GetActiveAsync();
            var parts = await _partService.GetAllAsync();
            var notes = await _noteService.GetByTicketIdAsync(id);

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
            var result = await _noteService.AddAsync(new TicketNoteFormDto
            {
                ServiceTicketId = id,
                Message = message
            });

            if (!result.Success)
                TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Failed to add note.";

            return RedirectToAction(nameof(Detail), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdvanceStatus(int id, ServiceTicketStatus newStatus)
        {
            var result = await _ticketService.UpdateStatusAsync(id, newStatus);
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
            var result = await _ticketService.UpdateDiagnosisAsync(id, diagnosisNotes, estimatedCompletion);
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
            if (dto.ServiceTypeId == null && dto.PartId == null)
            {
                TempData["Error"] = "Select a service type or a part.";
                return RedirectToAction(nameof(Detail), new { id });
            }

            var result = await _ticketService.AddItemAsync(id, dto);
            if (!result.Success)
                TempData["Error"] = result.Errors?.FirstOrDefault();
            else
                TempData["Success"] = "Item added.";

            return RedirectToAction(nameof(Detail), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveItem(int itemId, int ticketId)
        {
            var result = await _ticketService.RemoveItemAsync(itemId);
            if (!result.Success)
                TempData["Error"] = result.Errors?.FirstOrDefault();

            return RedirectToAction(nameof(Detail), new { id = ticketId });
        }

        // ── Leave Requests ──────────────────────────────────────────────────────

        // GET: /Mechanic/Leave
        public async Task<IActionResult> Leave()
        {
            var result = await _leaveService.GetMyLeaveRequestsAsync();
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
            var result = await _leaveService.SubmitAsync(dto);

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
            var result = await _leaveService.CancelAsync(id);
            if (!result.Success)
                TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Failed to cancel leave request.";
            else
                TempData["Success"] = result.Message;

            return RedirectToAction(nameof(Leave));
        }

        // GET: /Mechanic/Payroll
        public async Task<IActionResult> Payroll()
        {
            var result = await _payrollService.GetMyPayrollAsync();
            if (!result.Success)
            {
                TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Failed to load payroll records.";
                return View(new List<PayrollRecordDto>());
            }
            return View(result.Data);
        }
    }
}
