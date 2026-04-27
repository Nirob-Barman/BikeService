using BikeService.Application.DTOs.AuditLog;
using BikeService.Application.Interfaces;
using BikeService.Application.Interfaces.Services;
using BikeService.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BikeService.Web.Controllers.Admin
{
    [Authorize(Roles = AppRoles.Admin)]
    [Route("Admin/[controller]")]
    public class AuditLogController : Controller
    {
        private readonly IAuditLogService _auditLogService;

        public AuditLogController(IAuditLogService auditLogService)
        {
            _auditLogService = auditLogService;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index(
            string? entityName,
            string? action,
            string? userEmail,
            DateTime? dateFrom,
            DateTime? dateTo,
            int page = 1)
        {
            var filter = new AuditLogFilterDto
            {
                EntityName = entityName,
                Action = action,
                UserEmail = userEmail,
                DateFrom = dateFrom,
                DateTo = dateTo,
                PageNumber = page,
                PageSize = 20
            };

            var result = await _auditLogService.GetPagedAsync(filter);
            if (!result.Success)
            {
                TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Failed to load audit logs.";
                return View((Items: new List<AuditLogDto>(), TotalCount: 0, Filter: filter));
            }

            ViewBag.TotalCount = result.Data.TotalCount;
            ViewBag.PageNumber = page;
            ViewBag.PageSize = filter.PageSize;
            ViewBag.Filter = filter;

            return View((Items: result.Data.Items, TotalCount: result.Data.TotalCount, Filter: filter));
        }
    }
}
