using BikeService.Application.DTOs.Payment;
using BikeService.Application.Wrappers;

namespace BikeService.Application.Interfaces.Services
{
    public interface IPaymentService
    {
        Task<Result<CheckoutInfoDto>> GetCheckoutInfoAsync(int invoiceId, string? promoCode);
        Task<Result<string>> InitiateAsync(int invoiceId, int gatewayId, string? promoCode);
        Task<Result<bool>> HandleSuccessAsync(int txId, Dictionary<string, string> callbackParams);
        Task<Result<bool>> HandleCancelAsync(int txId);
    }
}
