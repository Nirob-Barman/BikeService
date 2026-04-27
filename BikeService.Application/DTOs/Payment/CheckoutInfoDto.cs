using BikeService.Application.DTOs.PaymentGateway;

namespace BikeService.Application.DTOs.Payment
{
    public class CheckoutInfoDto
    {
        public int InvoiceId { get; set; }
        public string BikeSummary { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal FinalAmount { get; set; }
        public string? AppliedPromoCode { get; set; }
        public decimal PromoDiscountPercent { get; set; }
        public List<PaymentGatewayDto> Gateways { get; set; } = new();
    }
}
