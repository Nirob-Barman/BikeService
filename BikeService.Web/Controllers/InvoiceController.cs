using BikeService.Application.DTOs.Invoice;
using BikeService.Application.Interfaces;
using BikeService.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BikeService.Web.Controllers
{
    [Authorize(Roles = "Customer")]
    public class InvoiceController : Controller
    {
        private readonly IInvoiceService _invoiceService;
        private readonly IPdfService _pdfService;

        public InvoiceController(IInvoiceService invoiceService, IPdfService pdfService)
        {
            _invoiceService = invoiceService;
            _pdfService = pdfService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var result = await _invoiceService.GetMyInvoicesAsync();
            if (!result.Success)
            {
                TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Failed to load invoices.";
                return View(new List<InvoiceDto>());
            }
            return View(result.Data);
        }

        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            var result = await _invoiceService.GetMyInvoiceByIdAsync(id);
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
            var result = await _invoiceService.GetMyInvoiceByIdAsync(id);
            if (!result.Success)
            {
                TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Invoice not found.";
                return RedirectToAction(nameof(Index));
            }

            var pdf = _pdfService.GenerateInvoicePdf(result.Data!);
            return File(pdf, "application/pdf", $"Invoice-{id}.pdf");
        }
    }
}
