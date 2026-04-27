using BikeService.Domain.Enums;

namespace BikeService.Domain.Entities;

public class PaymentTransaction : BaseEntity
{
    public decimal Amount { get; set; }
    public string? SessionRef { get; set; }
    public PaymentTransactionStatus Status { get; set; } = PaymentTransactionStatus.Pending;

    public int InvoiceId { get; set; }
    public Invoice Invoice { get; set; } = null!;

    public int GatewayId { get; set; }
    public PaymentGateway Gateway { get; set; } = null!;
}
