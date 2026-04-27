namespace BikeService.Domain.Entities;

public class PartStockAlert : BaseEntity
{
    public bool IsResolved { get; set; }

    public int PartId { get; set; }
    public Part Part { get; set; } = null!;
}
