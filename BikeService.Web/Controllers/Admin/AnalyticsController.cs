using BikeService.Application.Features.Dashboard.Queries.GetDashboard;
using BikeService.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BikeService.Web.Controllers.Admin
{
    [Authorize(Roles = AppRoles.Admin)]
    [Route("Admin/[controller]")]
    public class AnalyticsController : Controller
    {
        private readonly IMediator _mediator;

        public AnalyticsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var result = await _mediator.Send(new GetDashboardQuery());
            if (!result.Success)
            {
                TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Failed to load dashboard data.";
                return View(null);
            }
            return View(result.Data);
        }
    }
}
