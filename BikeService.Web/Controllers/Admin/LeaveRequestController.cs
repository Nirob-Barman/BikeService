using BikeService.Application.Interfaces.Services;
using BikeService.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BikeService.Web.Controllers.Admin
{
    [Authorize(Roles = AppRoles.Admin)]
    [Route("Admin/[controller]")]
    public class LeaveRequestController : Controller
    {
        private readonly ILeaveRequestService _leaveService;
        private readonly IMechanicService _mechanicService;

        public LeaveRequestController(ILeaveRequestService leaveService, IMechanicService mechanicService)
        {
            _leaveService    = leaveService;
            _mechanicService = mechanicService;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var result = await _leaveService.GetAllAsync();
            if (!result.Success)
            {
                TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Failed to load leave requests.";
                return View(new List<BikeService.Application.DTOs.LeaveRequest.LeaveRequestDto>());
            }
            return View(result.Data);
        }

        [HttpGet("Detail/{id}")]
        public async Task<IActionResult> Detail(int id)
        {
            var result = await _leaveService.GetByIdAsync(id);
            if (!result.Success)
            {
                TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Leave request not found.";
                return RedirectToAction(nameof(Index));
            }
            return View(result.Data);
        }

        [HttpPost("Approve/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id, string? adminNotes)
        {
            var result = await _leaveService.ApproveAsync(id, adminNotes);
            if (!result.Success)
                TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Failed to approve leave request.";
            else
                TempData["Success"] = result.Message;

            return RedirectToAction(nameof(Detail), new { id });
        }

        [HttpPost("Reject/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id, string? adminNotes)
        {
            var result = await _leaveService.RejectAsync(id, adminNotes);
            if (!result.Success)
                TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Failed to reject leave request.";
            else
                TempData["Success"] = result.Message;

            return RedirectToAction(nameof(Detail), new { id });
        }
    }
}
