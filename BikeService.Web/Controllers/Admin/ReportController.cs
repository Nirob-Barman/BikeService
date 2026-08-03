using BikeService.Application.DTOs.Report;
using BikeService.Application.Features.Reports.Queries.ExportInvoicesCsv;
using BikeService.Application.Features.Reports.Queries.ExportPartUsageCsv;
using BikeService.Application.Features.Reports.Queries.ExportTicketsCsv;
using BikeService.Application.Features.Reports.Queries.GetPartUsageReport;
using BikeService.Application.Features.Reports.Queries.GetRevenueReport;
using BikeService.Application.Features.Reports.Queries.GetTicketReport;
using BikeService.Web.ViewModels.Report;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace BikeService.Web.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    [Route("Admin/[controller]")]
    public class ReportController : Controller
    {
        private readonly IMediator _mediator;

        public ReportController(IMediator mediator)
        {
            _mediator = mediator;
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
            var revenue = await _mediator.Send(new GetRevenueReportQuery(filter));
            var tickets = await _mediator.Send(new GetTicketReportQuery(filter));
            var parts   = await _mediator.Send(new GetPartUsageReportQuery(filter));

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
            var csv = await _mediator.Send(new ExportInvoicesCsvQuery(new ReportFilterDto { DateFrom = dateFrom, DateTo = dateTo }));
            return File(Encoding.UTF8.GetBytes(csv), "text/csv", $"Invoices_{dateFrom:yyyyMMdd}_{dateTo:yyyyMMdd}.csv");
        }

        [HttpGet("ExportTickets")]
        public async Task<IActionResult> ExportTickets(DateTime dateFrom, DateTime dateTo)
        {
            var csv = await _mediator.Send(new ExportTicketsCsvQuery(new ReportFilterDto { DateFrom = dateFrom, DateTo = dateTo }));
            return File(Encoding.UTF8.GetBytes(csv), "text/csv", $"Tickets_{dateFrom:yyyyMMdd}_{dateTo:yyyyMMdd}.csv");
        }

        [HttpGet("ExportParts")]
        public async Task<IActionResult> ExportParts(DateTime dateFrom, DateTime dateTo)
        {
            var csv = await _mediator.Send(new ExportPartUsageCsvQuery(new ReportFilterDto { DateFrom = dateFrom, DateTo = dateTo }));
            return File(Encoding.UTF8.GetBytes(csv), "text/csv", $"PartUsage_{dateFrom:yyyyMMdd}_{dateTo:yyyyMMdd}.csv");
        }
    }
}
