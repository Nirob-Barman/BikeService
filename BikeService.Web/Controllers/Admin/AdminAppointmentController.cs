using BikeService.Application.DTOs.Appointment;
using BikeService.Application.DTOs.ServiceTicket;
using BikeService.Application.Interfaces.Services;
using BikeService.Domain.Constants;
using BikeService.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BikeService.Web.Controllers.Admin
{
    [Authorize(Roles = AppRoles.Admin)]
    [Route("Admin/Appointment/[action]/{id?}")]
    public class AdminAppointmentController : Controller
    {
        private readonly IAppointmentService _appointmentService;
        private readonly IServiceTicketService _ticketService;

        public AdminAppointmentController(IAppointmentService appointmentService, IServiceTicketService ticketService)
        {
            _appointmentService = appointmentService;
            _ticketService      = ticketService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(AppointmentStatus? status, DateTime? dateFrom, DateTime? dateTo)
        {
            var filter = new AppointmentFilterDto { Status = status, DateFrom = dateFrom, DateTo = dateTo };
            var result = await _appointmentService.GetAllAsync(filter);
            if (!result.Success)
            {
                TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Failed to load appointments.";
                return View(new List<AppointmentDto>());
            }

            ViewBag.StatusFilter = status;
            ViewBag.DateFrom     = dateFrom?.ToString("yyyy-MM-dd");
            ViewBag.DateTo       = dateTo?.ToString("yyyy-MM-dd");
            return View(result.Data);
        }

        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            var result = await _appointmentService.GetByIdAsync(id);
            if (!result.Success)
            {
                TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Appointment not found.";
                return RedirectToAction(nameof(Index));
            }
            return View(result.Data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirm(int id)
        {
            var result = await _appointmentService.ConfirmAsync(id);
            if (!result.Success)
                TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Failed to confirm appointment.";
            else
                TempData["Success"] = "Appointment confirmed.";

            return RedirectToAction(nameof(Detail), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var result = await _appointmentService.CancelAsync(id);
            if (!result.Success)
                TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Failed to cancel appointment.";
            else
                TempData["Success"] = "Appointment cancelled.";

            return RedirectToAction(nameof(Detail), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTicket(int id)
        {
            var appointmentResult = await _appointmentService.GetByIdAsync(id);
            if (!appointmentResult.Success)
            {
                TempData["Error"] = appointmentResult.Errors?.FirstOrDefault() ?? "Appointment not found.";
                return RedirectToAction(nameof(Detail), new { id });
            }

            var appointment = appointmentResult.Data!;
            var dto = new ServiceTicketFormDto { BikeId = appointment.BikeId, AppointmentId = appointment.Id };

            var result = await _ticketService.CreateAsync(dto);
            if (!result.Success)
            {
                TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Failed to create service ticket.";
                return RedirectToAction(nameof(Detail), new { id });
            }

            await _appointmentService.CompleteAsync(id);

            TempData["Success"] = "Service ticket created.";
            return RedirectToAction("Detail", "Ticket", new { id = result.Data });
        }
    }
}
