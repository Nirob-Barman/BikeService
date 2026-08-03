using BikeService.Application.Features.Dashboard.Queries.GetDashboard;
using BikeService.Application.Features.Mechanics.Queries.GetMechanics;
using BikeService.Application.Features.PromoCodes.Queries.GetActivePromoCodes;
using BikeService.Application.Features.Reviews.Queries.GetRecentReviews;
using BikeService.Application.Features.ServiceTypes.Queries.GetActiveServiceTypes;
using BikeService.Web.Models;
using BikeService.Web.ViewModels.Home;
using MediatR;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BikeService.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IMediator _mediator;

        public HomeController(ILogger<HomeController> logger, IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        public async Task<IActionResult> Index()
        {
            var serviceTypes = await _mediator.Send(new GetActiveServiceTypesQuery());
            var mechanics = await _mediator.Send(new GetMechanicsQuery());
            var promoCodes = await _mediator.Send(new GetActivePromoCodesQuery());
            var dashboard = await _mediator.Send(new GetDashboardQuery());
            var reviews = await _mediator.Send(new GetRecentReviewsQuery(6));

            var vm = new HomeViewModel
            {
                ServiceTypes = serviceTypes.Data ?? new(),
                Mechanics = (mechanics.Data ?? new()).Take(4).ToList(),
                PromoCodes = promoCodes.Data ?? new(),
                Reviews = reviews.Data ?? new(),
                TotalBikesServiced = dashboard.Data?.TotalBikes ?? 0,
                TotalCustomers = dashboard.Data?.TotalCustomers ?? 0,
                CompletedTickets = (dashboard.Data?.ActiveTickets ?? 0) + (dashboard.Data?.TicketsToday ?? 0),
            };

            return View(vm);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(int? statusCode)
        {
            var exceptionFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

            if (exceptionFeature?.Error is not null)
                _logger.LogError(exceptionFeature.Error, "Unhandled exception at {Path}", exceptionFeature.Path);

            var model = new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                StatusCode = statusCode,
                Message = statusCode switch
                {
                    404 => "The page you're looking for doesn't exist.",
                    403 => "You don't have permission to access this page.",
                    _ => "Something went wrong. Please try again later."
                }
            };

            return View(model);
        }
    }
}
