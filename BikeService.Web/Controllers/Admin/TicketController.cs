using BikeService.Application.DTOs.ServiceTicket;
using BikeService.Application.Interfaces.Services;
using BikeService.Domain.Constants;
using BikeService.Domain.Enums;
using BikeService.Web.ViewModels.ServiceTicket;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BikeService.Web.Controllers.Admin
{
    [Authorize(Roles = AppRoles.Admin)]
    [Route("Admin/[controller]/[action]/{id?}")]
    public class TicketController : Controller
    {
        private readonly IServiceTicketService _ticketService;
        private readonly IMechanicService _mechanicService;
        private readonly IServiceTypeService _serviceTypeService;
        private readonly IPartService _partService;
        private readonly IInvoiceService _invoiceService;
        private readonly ICustomerBikeService _bikeService;

        public TicketController(
            IServiceTicketService ticketService,
            IMechanicService mechanicService,
            IServiceTypeService serviceTypeService,
            IPartService partService,
            IInvoiceService invoiceService,
            ICustomerBikeService bikeService)
        {
            _ticketService      = ticketService;
            _mechanicService    = mechanicService;
            _serviceTypeService = serviceTypeService;
            _partService        = partService;
            _invoiceService     = invoiceService;
            _bikeService        = bikeService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(ServiceTicketStatus? status, int? mechanicId, DateTime? dateFrom, DateTime? dateTo)
        {
            var filter = new TicketFilterDto { Status = status, MechanicId = mechanicId, DateFrom = dateFrom, DateTo = dateTo };
            var result = await _ticketService.GetAllAsync(filter);
            if (!result.Success)
            {
                TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Failed to load tickets.";
                return View(new List<ServiceTicketDto>());
            }

            var mechanicsResult = await _mechanicService.GetAvailableAsync();
            ViewBag.Mechanics        = mechanicsResult.Success ? mechanicsResult.Data : new List<Application.DTOs.Mechanic.MechanicDto>();
            ViewBag.StatusFilter     = status;
            ViewBag.MechanicIdFilter = mechanicId;
            ViewBag.DateFrom         = dateFrom?.ToString("yyyy-MM-dd");
            ViewBag.DateTo           = dateTo?.ToString("yyyy-MM-dd");

            return View(result.Data);
        }

        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            var ticketResult = await _ticketService.GetByIdAsync(id);
            if (!ticketResult.Success)
            {
                TempData["Error"] = ticketResult.Errors?.FirstOrDefault() ?? "Ticket not found.";
                return RedirectToAction(nameof(Index));
            }

            var mechanicsResult    = await _mechanicService.GetAvailableAsync();
            var serviceTypesResult = await _serviceTypeService.GetActiveAsync();
            var partsResult        = await _partService.GetAllAsync();

            return View(new TicketDetailViewModel
            {
                Ticket             = ticketResult.Data!,
                AvailableMechanics = mechanicsResult.Success ? mechanicsResult.Data! : new(),
                ActiveServiceTypes = serviceTypesResult.Success ? serviceTypesResult.Data! : new(),
                AllParts           = partsResult.Success ? partsResult.Data! : new()
            });
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await PopulateCreateDropdowns();
            return View(new WalkInTicketFormViewModel { EstimatedCompletionDate = DateTime.Today.AddDays(3) });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(WalkInTicketFormViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                await PopulateCreateDropdowns();
                return View(vm);
            }

            var dto = new ServiceTicketFormDto
            {
                BikeId                  = vm.BikeId,
                MechanicId              = vm.MechanicId,
                DiagnosisNotes          = vm.DiagnosisNotes,
                EstimatedCompletionDate = vm.EstimatedCompletionDate,
            };

            var result = await _ticketService.CreateAsync(dto);
            if (!result.Success)
            {
                TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Failed to create ticket.";
                await PopulateCreateDropdowns();
                return View(vm);
            }

            TempData["Success"] = "Walk-in ticket created successfully.";
            return RedirectToAction(nameof(Detail), new { id = result.Data });
        }

        private async Task PopulateCreateDropdowns()
        {
            var bikes     = await _bikeService.GetAllAsync();
            var mechanics = await _mechanicService.GetAllAsync();

            ViewBag.Bikes = new SelectList(
                (bikes.Data ?? []).Select(b => new
                {
                    b.Id,
                    Label = $"{b.Make} {b.Model} ({b.Year})" + (b.RegistrationNo != null ? $" — {b.RegistrationNo}" : "")
                }),
                "Id", "Label");

            ViewBag.Mechanics = new SelectList(mechanics.Data ?? [], "Id", "FullName");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, ServiceTicketStatus newStatus)
        {
            var result = await _ticketService.UpdateStatusAsync(id, newStatus);
            if (!result.Success)
                TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Failed to update status.";
            else
                TempData["Success"] = "Ticket status updated.";

            return RedirectToAction(nameof(Detail), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignMechanic(int id, int mechanicId)
        {
            var result = await _ticketService.AssignMechanicAsync(id, mechanicId);
            if (!result.Success)
                TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Failed to assign mechanic.";
            else
                TempData["Success"] = "Mechanic assigned successfully.";

            return RedirectToAction(nameof(Detail), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateDiagnosis(int id, string? diagnosisNotes, DateTime? estimatedCompletionDate)
        {
            var result = await _ticketService.UpdateDiagnosisAsync(id, diagnosisNotes, estimatedCompletionDate);
            if (!result.Success)
                TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Failed to update diagnosis.";
            else
                TempData["Success"] = "Diagnosis notes saved.";

            return RedirectToAction(nameof(Detail), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddItem(int ticketId, ServiceTicketItemFormDto dto)
        {
            var result = await _ticketService.AddItemAsync(ticketId, dto);
            if (!result.Success)
                TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Failed to add item.";
            else
                TempData["Success"] = "Item added to ticket.";

            return RedirectToAction(nameof(Detail), new { id = ticketId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveItem(int itemId, int ticketId)
        {
            var result = await _ticketService.RemoveItemAsync(itemId);
            if (!result.Success)
                TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Failed to remove item.";
            else
                TempData["Success"] = "Item removed from ticket.";

            return RedirectToAction(nameof(Detail), new { id = ticketId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var result = await _ticketService.CancelAsync(id);
            if (!result.Success)
                TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Failed to cancel ticket.";
            else
                TempData["Success"] = "Ticket cancelled.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateInvoice(int id)
        {
            var result = await _invoiceService.GenerateAsync(id);
            if (!result.Success)
                TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Failed to generate invoice.";
            else
                TempData["Success"] = "Invoice generated successfully.";

            return RedirectToAction(nameof(Detail), new { id });
        }
    }
}
