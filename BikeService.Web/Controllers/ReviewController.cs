using BikeService.Application.Features.Reviews.Commands.CreateReview;
using BikeService.Application.Features.Reviews.Queries.GetReviewByTicketId;
using BikeService.Application.Features.ServiceTickets.Queries.GetServiceTicketById;
using BikeService.Domain.Constants;
using BikeService.Web.ViewModels.Review;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BikeService.Web.Controllers
{
    [Authorize(Roles = AppRoles.Customer)]
    public class ReviewController : Controller
    {
        private readonly IMediator _mediator;

        public ReviewController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> Create(int ticketId)
        {
            // Check ticket exists and is delivered
            var ticketResult = await _mediator.Send(new GetServiceTicketByIdQuery(ticketId));
            if (!ticketResult.Success)
            {
                TempData["Error"] = "Ticket not found.";
                return RedirectToAction("Index", "ServiceTicket");
            }

            var ticket = ticketResult.Data!;

            // Check no existing review
            var existing = await _mediator.Send(new GetReviewByTicketIdQuery(ticketId));
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

            var result = await _mediator.Send(new CreateReviewCommand(
                vm.ServiceTicketId, vm.Rating, vm.Comment));

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
