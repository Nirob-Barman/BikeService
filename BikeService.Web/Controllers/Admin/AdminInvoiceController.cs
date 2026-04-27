using BikeService.Application.DTOs.Invoice;
using BikeService.Application.Interfaces;
using BikeService.Application.Interfaces.Services;
using BikeService.Domain.Constants;
using BikeService.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BikeService.Web.Controllers.Admin
{
    [Authorize(Roles = AppRoles.Admin)]
    [Route("Admin/Invoice/[action]/{id?}")]
    public class AdminInvoiceController : Controller
    {
        private readonly IInvoiceService _invoiceService;
        private readonly IPdfService _pdfService;

        public AdminInvoiceController(IInvoiceService invoiceService, IPdfService pdfService)
        {
            _invoiceService = invoiceService;
            _pdfService     = pdfService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(InvoiceStatus? status)
        {
            var result = await _invoiceService.GetAllAsync(new InvoiceFilterDto { Status = status });
            if (!result.Success)
            {
                TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Failed to load invoices.";
                return View(new List<InvoiceDto>());
            }

            ViewBag.StatusFilter = status;
            return View(result.Data);
        }

        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            var result = await _invoiceService.GetByIdAsync(id);
            if (!result.Success)
            {
                TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Invoice not found.";
                return RedirectToAction(nameof(Index));
            }
            return View(result.Data);
        }

        [HttpGet]
        public async Task<IActionResult> Download(int id)
        {
            var result = await _invoiceService.GetByIdAsync(id);
            if (!result.Success)
            {
                TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Invoice not found.";
                return RedirectToAction(nameof(Detail), new { id });
            }
            var pdf = _pdfService.GenerateInvoicePdf(result.Data!);
            return File(pdf, "application/pdf", $"Invoice-{id}.pdf");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Issue(int id)
        {
            var result = await _invoiceService.IssueAsync(id);
            if (!result.Success)
                TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Failed to issue invoice.";
            else
                TempData["Success"] = "Invoice issued successfully.";

            return RedirectToAction(nameof(Detail), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Void(int id)
        {
            var result = await _invoiceService.VoidAsync(id);
            if (!result.Success)
                TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Failed to void invoice.";
            else
                TempData["Success"] = "Invoice voided.";

            return RedirectToAction(nameof(Detail), new { id });
        }
    }
}
