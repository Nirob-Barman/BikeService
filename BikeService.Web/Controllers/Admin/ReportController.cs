using BikeService.Application.DTOs.Report;
using BikeService.Application.Interfaces.Services;
using BikeService.Web.ViewModels.Report;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace BikeService.Web.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    [Route("Admin/[controller]")]
    public class ReportController : Controller
    {
        private readonly IReportService _reportService;

        public ReportController(IReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpGet("")]
        public IActionResult Index() => View(new ReportViewModel());

        [HttpPost("")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(DateTime dateFrom, DateTime dateTo)
        {
            if (dateFrom > dateTo)
            {
                ModelState.AddModelError("", "Start date must be before end date.");
                return View(new ReportViewModel { DateFrom = dateFrom, DateTo = dateTo });
            }

            var filter = new ReportFilterDto { DateFrom = dateFrom, DateTo = dateTo };
            var revenue = await _reportService.GetRevenueReportAsync(filter);
            var tickets = await _reportService.GetTicketReportAsync(filter);
            var parts   = await _reportService.GetPartUsageReportAsync(filter);

            return View(new ReportViewModel
            {
                DateFrom  = dateFrom,
                DateTo    = dateTo,
                Revenue   = revenue.Data,
                Tickets   = tickets.Data,
                PartUsage = parts.Data ?? new()
            });
        }

        [HttpGet("ExportInvoices")]
        public async Task<IActionResult> ExportInvoices(DateTime dateFrom, DateTime dateTo)
        {
            var csv = await _reportService.ExportInvoicesCsvAsync(new ReportFilterDto { DateFrom = dateFrom, DateTo = dateTo });
            return File(Encoding.UTF8.GetBytes(csv), "text/csv", $"Invoices_{dateFrom:yyyyMMdd}_{dateTo:yyyyMMdd}.csv");
        }

        [HttpGet("ExportTickets")]
        public async Task<IActionResult> ExportTickets(DateTime dateFrom, DateTime dateTo)
        {
            var csv = await _reportService.ExportTicketsCsvAsync(new ReportFilterDto { DateFrom = dateFrom, DateTo = dateTo });
            return File(Encoding.UTF8.GetBytes(csv), "text/csv", $"Tickets_{dateFrom:yyyyMMdd}_{dateTo:yyyyMMdd}.csv");
        }

        [HttpGet("ExportParts")]
        public async Task<IActionResult> ExportParts(DateTime dateFrom, DateTime dateTo)
        {
            var csv = await _reportService.ExportPartUsageCsvAsync(new ReportFilterDto { DateFrom = dateFrom, DateTo = dateTo });
            return File(Encoding.UTF8.GetBytes(csv), "text/csv", $"PartUsage_{dateFrom:yyyyMMdd}_{dateTo:yyyyMMdd}.csv");
        }
    }
}
