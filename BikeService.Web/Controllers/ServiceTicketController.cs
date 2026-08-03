using BikeService.Application.DTOs.ServiceTicket;
using BikeService.Application.Features.ServiceTickets.Queries.GetMyServiceTickets;
using BikeService.Application.Features.ServiceTickets.Queries.GetMyServiceTicketById;
using BikeService.Application.Features.TicketNotes.Commands.AddTicketNote;
using BikeService.Application.Features.TicketNotes.Queries.GetTicketNotes;
using BikeService.Domain.Constants;
using BikeService.Web.ViewModels.ServiceTicket;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BikeService.Web.Controllers
{
    [Authorize(Roles = AppRoles.Customer)]
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
            var ticketResult = await _mediator.Send(new GetMyServiceTicketByIdQuery(id));
            if (!ticketResult.Success)
            {
                TempData["Error"] = ticketResult.Errors?.FirstOrDefault() ?? "Ticket not found.";
                return RedirectToAction(nameof(Index));
            }

            var notesResult = await _mediator.Send(new GetTicketNotesQuery(id));

            return View(new ServiceTicketDetailViewModel
            {
                Ticket = ticketResult.Data!,
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
