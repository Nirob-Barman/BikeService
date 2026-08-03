using BikeService.Application.Features.PromoCodes.Queries.GetActivePromoCodes;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BikeService.Web.Controllers
{
    public class OfferController : Controller
    {
        private readonly IMediator _mediator;

        public OfferController(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<IActionResult> Index()
        {
            var result = await _mediator.Send(new GetActivePromoCodesQuery());
            return View(result.Data ?? new());
        }
    }
}
