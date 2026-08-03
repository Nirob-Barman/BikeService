using BikeService.Application.DTOs.Appointment;
using BikeService.Application.DTOs.ServiceTicket;
using BikeService.Application.Features.Appointments.Commands.CancelAppointment;
using BikeService.Application.Features.Appointments.Commands.CompleteAppointment;
using BikeService.Application.Features.Appointments.Commands.ConfirmAppointment;
using BikeService.Application.Features.Appointments.Queries.GetAppointmentById;
using BikeService.Application.Features.Appointments.Queries.GetAppointments;
using BikeService.Application.Features.ServiceTickets.Commands.CreateServiceTicket;
using BikeService.Domain.Constants;
using BikeService.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BikeService.Web.Controllers.Admin
{
    [Authorize(Roles = AppRoles.Admin)]
    [Route("Admin/Appointment")]
    public class AdminAppointmentController : Controller
    {
        private readonly IMediator _mediator;

        public AdminAppointmentController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index(AppointmentStatus? status, DateTime? dateFrom, DateTime? dateTo)
        {
            var filter = new AppointmentFilterDto { Status = status, DateFrom = dateFrom, DateTo = dateTo };
            var result = await _mediator.Send(new GetAppointmentsQuery(filter));
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

        [HttpGet("Detail/{id}")]
        public async Task<IActionResult> Detail(int id)
        {
            var result = await _mediator.Send(new GetAppointmentByIdQuery(id));
            if (!result.Success)
            {
                TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Appointment not found.";
                return RedirectToAction(nameof(Index));
            }
            return View(result.Data);
        }

        [HttpPost("Confirm/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirm(int id)
        {
            var result = await _mediator.Send(new ConfirmAppointmentCommand(id));
            if (!result.Success)
                TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Failed to confirm appointment.";
            else
                TempData["Success"] = "Appointment confirmed.";

            return RedirectToAction(nameof(Detail), new { id });
        }

        [HttpPost("Cancel/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var result = await _mediator.Send(new CancelAppointmentCommand(id));
            if (!result.Success)
                TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Failed to cancel appointment.";
            else
                TempData["Success"] = "Appointment cancelled.";

            return RedirectToAction(nameof(Detail), new { id });
        }

        [HttpPost("CreateTicket/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTicket(int id)
        {
            var appointmentResult = await _mediator.Send(new GetAppointmentByIdQuery(id));
            if (!appointmentResult.Success)
            {
                TempData["Error"] = appointmentResult.Errors?.FirstOrDefault() ?? "Appointment not found.";
                return RedirectToAction(nameof(Detail), new { id });
            }

            var appointment = appointmentResult.Data!;

            var result = await _mediator.Send(new CreateServiceTicketCommand(
                appointment.BikeId, null, appointment.Id, null, null));
            if (!result.Success)
            {
                TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Failed to create service ticket.";
                return RedirectToAction(nameof(Detail), new { id });
            }

            await _mediator.Send(new CompleteAppointmentCommand(id));

            TempData["Success"] = "Service ticket created.";
            return RedirectToAction("Detail", "Ticket", new { id = result.Data });
        }
    }
}
