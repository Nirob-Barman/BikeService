using BikeService.Application.Interfaces.Services;
using BikeService.Web.Models;
using BikeService.Web.ViewModels.Home;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BikeService.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IServiceTypeService _serviceTypeService;
        private readonly IMechanicService _mechanicService;
        private readonly IPromoCodeService _promoCodeService;
        private readonly IDashboardService _dashboardService;
        private readonly IReviewService _reviewService;

        public HomeController(
            ILogger<HomeController> logger,
            IServiceTypeService serviceTypeService,
            IMechanicService mechanicService,
            IPromoCodeService promoCodeService,
            IDashboardService dashboardService,
            IReviewService reviewService)
        {
            _logger = logger;
            _serviceTypeService = serviceTypeService;
            _mechanicService = mechanicService;
            _promoCodeService = promoCodeService;
            _dashboardService = dashboardService;
            _reviewService = reviewService;
        }

        public async Task<IActionResult> Index()
        {
            var serviceTypes = await _serviceTypeService.GetActiveAsync();
            var mechanics = await _mechanicService.GetAllAsync();
            var promoCodes = await _promoCodeService.GetActiveAsync();
            var dashboard = await _dashboardService.GetDashboardAsync();
            var reviews = await _reviewService.GetRecentAsync(6);

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
