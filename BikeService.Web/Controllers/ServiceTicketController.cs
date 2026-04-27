using BikeService.Application.DTOs.ServiceTicket;
using BikeService.Application.DTOs.TicketNote;
using BikeService.Application.Interfaces.Services;
using BikeService.Web.ViewModels.ServiceTicket;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BikeService.Web.Controllers
{
    [Authorize(Roles = "Customer")]
    public class ServiceTicketController : Controller
    {
        private readonly IServiceTicketService _ticketService;
        private readonly ITicketNoteService _noteService;

        public ServiceTicketController(IServiceTicketService ticketService, ITicketNoteService noteService)
        {
            _ticketService = ticketService;
            _noteService = noteService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var result = await _ticketService.GetMyTicketsAsync();
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
            var ticketResult = await _ticketService.GetByIdAsync(id);
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

            var notesResult = await _noteService.GetByTicketIdAsync(id);

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
            var result = await _noteService.AddAsync(new TicketNoteFormDto
            {
                ServiceTicketId = id,
                Message = message
            });

            if (!result.Success)
                TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Failed to add note.";

            return RedirectToAction(nameof(Detail), new { id });
        }
    }
}
