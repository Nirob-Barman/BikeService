using BikeService.Application.DTOs.Review;
using BikeService.Application.Interfaces.Services;
using BikeService.Web.ViewModels.Review;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BikeService.Web.Controllers
{
    [Authorize(Roles = "Customer")]
    public class ReviewController : Controller
    {
        private readonly IReviewService _reviewService;
        private readonly IServiceTicketService _ticketService;

        public ReviewController(IReviewService reviewService, IServiceTicketService ticketService)
        {
            _reviewService = reviewService;
            _ticketService = ticketService;
        }

        [HttpGet]
        public async Task<IActionResult> Create(int ticketId)
        {
            // Check ticket exists and is delivered
            var ticketResult = await _ticketService.GetByIdAsync(ticketId);
            if (!ticketResult.Success)
            {
                TempData["Error"] = "Ticket not found.";
                return RedirectToAction("Index", "ServiceTicket");
            }

            var ticket = ticketResult.Data!;

            // Check no existing review
            var existing = await _reviewService.GetByTicketIdAsync(ticketId);
            if (existing.Success && existing.Data != null)
            {
                TempData["Error"] = "You have already reviewed this service.";
                return RedirectToAction("Detail", "ServiceTicket", new { id = ticketId });
            }

            return View(new ReviewFormViewModel
            {
                ServiceTicketId = ticketId,
                BikeSummary = ticket.BikeSummary
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ReviewFormViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var result = await _reviewService.CreateAsync(new ReviewFormDto
            {
                ServiceTicketId = vm.ServiceTicketId,
                Rating = vm.Rating,
                Comment = vm.Comment
            });

            if (!result.Success)
            {
                if (result.FieldErrors != null)
                    foreach (var fe in result.FieldErrors)
                        ModelState.AddModelError(fe.Key, fe.Value);
                else
                    ModelState.AddModelError("", result.Errors?.FirstOrDefault() ?? "Failed to submit review.");

                return View(vm);
            }

            TempData["Success"] = "Thank you for your review!";
            return RedirectToAction("Detail", "ServiceTicket", new { id = vm.ServiceTicketId });
        }
    }
}
