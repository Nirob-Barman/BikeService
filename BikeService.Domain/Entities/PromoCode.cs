namespace BikeService.Domain.Entities;

public class PromoCode : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public decimal DiscountPercent { get; set; }
    public int MaxUsages { get; set; }
    public int UsageCount { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Invoice> Invoices { get; set; } = [];
}
