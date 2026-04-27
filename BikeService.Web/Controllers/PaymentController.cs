using BikeService.Application.Interfaces.Services;
using BikeService.Web.ViewModels.Payment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BikeService.Web.Controllers
{
    [Authorize(Roles = "Customer")]
    public class PaymentController : Controller
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpGet]
        public async Task<IActionResult> Checkout(int invoiceId, string? promoCode)
        {
            var result = await _paymentService.GetCheckoutInfoAsync(invoiceId, promoCode);
            if (!result.Success)
            {
                TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Unable to load checkout.";
                return RedirectToAction("Index", "Invoice");
            }

            var vm = new CheckoutViewModel
            {
                Info = result.Data!,
                PromoCode = promoCode
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Initiate(CheckoutViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                // Reload checkout info
                var reload = await _paymentService.GetCheckoutInfoAsync(vm.Info.InvoiceId, vm.PromoCode);
                if (reload.Success) vm.Info = reload.Data!;
                return View("Checkout", vm);
            }

            var result = await _paymentService.InitiateAsync(vm.Info.InvoiceId, vm.GatewayId, vm.PromoCode);
            if (!result.Success)
            {
                TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Payment initiation failed.";
                return RedirectToAction("Checkout", new { invoiceId = vm.Info.InvoiceId });
            }

            return Redirect(result.Data!);
        }

        [HttpGet]
        [AllowAnonymous] // Gateway callbacks may not carry auth cookie
        public async Task<IActionResult> Success(int txId, string gateway)
        {
            var callbackParams = Request.Query
                .ToDictionary(kv => kv.Key, kv => kv.Value.ToString());

            var result = await _paymentService.HandleSuccessAsync(txId, callbackParams);
            ViewBag.Success = result.Success;
            ViewBag.Message = result.Success
                ? (result.Message ?? "Payment successful!")
                : (result.Errors?.FirstOrDefault() ?? "Payment could not be verified.");

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> SuccessPost(int txId, string gateway)
        {
            var callbackParams = Request.Form
                .ToDictionary(kv => kv.Key, kv => kv.Value.ToString());
            callbackParams["txId"] = txId.ToString();
            callbackParams["gateway"] = gateway;

            var result = await _paymentService.HandleSuccessAsync(txId, callbackParams);
            ViewBag.Success = result.Success;
            ViewBag.Message = result.Success
                ? (result.Message ?? "Payment successful!")
                : (result.Errors?.FirstOrDefault() ?? "Payment could not be verified.");

            return View("Success");
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Cancel(int txId)
        {
            await _paymentService.HandleCancelAsync(txId);
            return View();
        }
    }
}
