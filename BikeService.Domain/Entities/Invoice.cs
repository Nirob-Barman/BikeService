using BikeService.Domain.Enums;

namespace BikeService.Domain.Entities;

public class Invoice : BaseEntity
{
    public decimal TotalAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal FinalAmount { get; set; }
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;

    public int ServiceTicketId { get; set; }
    public ServiceTicket ServiceTicket { get; set; } = null!;

    public int? PromoCodeId { get; set; }
    public PromoCode? PromoCode { get; set; }

    public ICollection<PaymentTransaction> PaymentTransactions { get; set; } = [];
}
