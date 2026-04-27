namespace BikeService.Domain.Entities;

public class Part : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int StockQuantity { get; set; }
    public int LowStockThreshold { get; set; }

    public ICollection<ServiceTicketItem> ServiceTicketItems { get; set; } = [];
    public ICollection<PartStockAlert> StockAlerts { get; set; } = [];
}
