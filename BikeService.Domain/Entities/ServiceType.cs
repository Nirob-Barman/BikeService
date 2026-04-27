namespace BikeService.Domain.Entities;

public class ServiceType : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal BasePrice { get; set; }
    public double EstimatedHours { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<ServiceTicketItem> ServiceTicketItems { get; set; } = [];
}
