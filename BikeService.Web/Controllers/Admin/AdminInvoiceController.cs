using BikeService.Application.DTOs.Invoice;
using BikeService.Application.Features.Invoices.Commands.IssueInvoice;
using BikeService.Application.Features.Invoices.Commands.VoidInvoice;
using BikeService.Application.Features.Invoices.Queries.GetInvoiceById;
using BikeService.Application.Features.Invoices.Queries.GetInvoices;
using BikeService.Application.Interfaces;
using BikeService.Domain.Constants;
using BikeService.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BikeService.Web.Controllers.Admin
{
    [Authorize(Roles = AppRoles.Admin)]
    [Route("Admin/Invoice")]
    public class AdminInvoiceController : Controller
    {
        private readonly IMediator _mediator;
        private readonly IPdfService _pdfService;

        public AdminInvoiceController(IMediator mediator, IPdfService pdfService)
        {
            _mediator   = mediator;
            _pdfService = pdfService;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index(InvoiceStatus? status)
        {
            var result = await _mediator.Send(new GetInvoicesQuery(new InvoiceFilterDto { Status = status }));
            if (!result.Success)
            {
                TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Failed to load invoices.";
                return View(new List<InvoiceDto>());
            }

            ViewBag.StatusFilter = status;
            return View(result.Data);
        }

        [HttpGet("Detail/{id}")]
        public async Task<IActionResult> Detail(int id)
        {
            var result = await _mediator.Send(new GetInvoiceByIdQuery(id));
            if (!result.Success)
            {
                TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Invoice not found.";
                return RedirectToAction(nameof(Index));
            }
            return View(result.Data);
        }

        [HttpGet("Download/{id}")]
        public async Task<IActionResult> Download(int id)
        {
            var result = await _mediator.Send(new GetInvoiceByIdQuery(id));
            if (!result.Success)
            {
                TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Invoice not found.";
                return RedirectToAction(nameof(Detail), new { id });
            }
            var pdf = _pdfService.GenerateInvoicePdf(result.Data!);
            return File(pdf, "application/pdf", $"Invoice-{id}.pdf");
        }

        [HttpPost("Issue/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Issue(int id)
        {
            var result = await _mediator.Send(new IssueInvoiceCommand(id));
            if (!result.Success)
                TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Failed to issue invoice.";
            else
                TempData["Success"] = "Invoice issued successfully.";

            return RedirectToAction(nameof(Detail), new { id });
        }

        [HttpPost("Void/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Void(int id)
        {
            var result = await _mediator.Send(new VoidInvoiceCommand(id));
            if (!result.Success)
                TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Failed to void invoice.";
            else
                TempData["Success"] = "Invoice voided.";

            return RedirectToAction(nameof(Detail), new { id });
        }
    }
}
