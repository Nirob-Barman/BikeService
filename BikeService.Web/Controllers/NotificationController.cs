using BikeService.Application.Features.Notifications.Commands.MarkAllNotificationsAsRead;
using BikeService.Application.Features.Notifications.Commands.MarkNotificationAsRead;
using BikeService.Application.Features.Notifications.Queries.GetNotifications;
using BikeService.Application.Features.Notifications.Queries.GetUnreadNotificationCount;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BikeService.Web.Controllers
{
    [Authorize]
    public class NotificationController : Controller
    {
        private readonly IMediator _mediator;

        public NotificationController(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<IActionResult> Index()
        {
            var result = await _mediator.Send(new GetNotificationsQuery(50));
            await _mediator.Send(new MarkAllNotificationsAsReadCommand());
            return View(result.Data ?? new());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkRead(int id, string? returnUrl)
        {
            await _mediator.Send(new MarkNotificationAsReadCommand(id));
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAllRead()
        {
            await _mediator.Send(new MarkAllNotificationsAsReadCommand());
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> GetUnreadCount()
        {
            var count = await _mediator.Send(new GetUnreadNotificationCountQuery());
            return Json(new { count });
        }
    }
}
