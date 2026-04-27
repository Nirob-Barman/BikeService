using BikeService.Application.DTOs.Payment;
using BikeService.Application.Interfaces;

namespace BikeService.Infrastructure.Payments
{
    public class MockPaymentProcessor : IPaymentProcessor
    {
        public string Slug => "mock";

        public Task<PaymentInitiateResult> InitiateAsync(
            Dictionary<string, string> config,
            decimal amount,
            int txId,
            string successUrl,
            string cancelUrl)
        {
            // Demo mode: redirect immediately to the success callback
            return Task.FromResult(new PaymentInitiateResult
            {
                Success = true,
                RedirectUrl = successUrl
            });
        }

        public Task<bool> VerifyAsync(
            Dictionary<string, string> config,
            Dictionary<string, string> callbackParams)
        {
            return Task.FromResult(true);
        }
    }
}
