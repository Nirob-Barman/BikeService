using BikeService.Application.Interfaces.Services;
using BikeService.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BikeService.Web.Controllers.Admin
{
    [Authorize(Roles = AppRoles.Admin)]
    [Route("Admin/[controller]")]
    public class AnalyticsController : Controller
    {
        private readonly IDashboardService _dashboardService;

        public AnalyticsController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var result = await _dashboardService.GetDashboardAsync();
            if (!result.Success)
            {
                TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Failed to load dashboard data.";
                return View(null);
            }
            return View(result.Data);
        }
    }
}
