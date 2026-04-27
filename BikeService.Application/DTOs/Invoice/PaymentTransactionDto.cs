using BikeService.Domain.Enums;

namespace BikeService.Application.DTOs.Invoice
{
    public class PaymentTransactionDto
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public string? SessionRef { get; set; }
        public PaymentTransactionStatus Status { get; set; }
        public string StatusLabel => Status.ToString();
        public string GatewayName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
