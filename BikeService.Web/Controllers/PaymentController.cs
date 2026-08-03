using BikeService.Application.Features.Payments.Commands.HandlePaymentCancel;
using BikeService.Application.Features.Payments.Commands.HandlePaymentSuccess;
using BikeService.Application.Features.Payments.Commands.InitiatePayment;
using BikeService.Application.Features.Payments.Queries.GetCheckoutInfo;
using BikeService.Web.ViewModels.Payment;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BikeService.Web.Controllers
{
    [Authorize(Roles = "Customer")]
    public class PaymentController : Controller
    {
        private readonly IMediator _mediator;

        public PaymentController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> Checkout(int invoiceId, string? promoCode)
        {
            var result = await _mediator.Send(new GetCheckoutInfoQuery(invoiceId, promoCode));
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
                var reload = await _mediator.Send(new GetCheckoutInfoQuery(vm.Info.InvoiceId, vm.PromoCode));
                if (reload.Success) vm.Info = reload.Data!;
                return View("Checkout", vm);
            }

            var result = await _mediator.Send(new InitiatePaymentCommand(vm.Info.InvoiceId, vm.GatewayId, vm.PromoCode));
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

            var result = await _mediator.Send(new HandlePaymentSuccessCommand(txId, callbackParams));
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

            var result = await _mediator.Send(new HandlePaymentSuccessCommand(txId, callbackParams));
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
            await _mediator.Send(new HandlePaymentCancelCommand(txId));
            return View();
        }
    }
}
