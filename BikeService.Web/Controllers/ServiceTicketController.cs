using BikeService.Application.DTOs.ServiceTicket;
using BikeService.Application.Features.ServiceTickets.Queries.GetMyServiceTickets;
using BikeService.Application.Features.ServiceTickets.Queries.GetServiceTicketById;
using BikeService.Application.Features.TicketNotes.Commands.AddTicketNote;
using BikeService.Application.Features.TicketNotes.Queries.GetTicketNotes;
using BikeService.Web.ViewModels.ServiceTicket;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BikeService.Web.Controllers
{
    [Authorize(Roles = "Customer")]
    public class ServiceTicketController : Controller
    {
        private readonly IMediator _mediator;

        public ServiceTicketController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var result = await _mediator.Send(new GetMyServiceTicketsQuery());
            if (!result.Success)
            {
                TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Failed to load service tickets.";
                return View(new List<ServiceTicketDto>());
            }
            return View(result.Data);
        }

        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            var ticketResult = await _mediator.Send(new GetServiceTicketByIdQuery(id));
            if (!ticketResult.Success)
            {
                TempData["Error"] = "Ticket not found.";
                return RedirectToAction(nameof(Index));
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (ticketResult.Data!.CustomerId != userId)
            {
                TempData["Error"] = "Access denied.";
                return RedirectToAction(nameof(Index));
            }

            var notesResult = await _mediator.Send(new GetTicketNotesQuery(id));

            return View(new ServiceTicketDetailViewModel
            {
                Ticket = ticketResult.Data,
                Notes = notesResult.Data ?? new()
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
    }
}
