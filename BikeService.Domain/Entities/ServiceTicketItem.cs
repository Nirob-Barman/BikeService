namespace BikeService.Domain.Entities;

public class ServiceTicketItem : BaseEntity
{
    public int Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }

    public int ServiceTicketId { get; set; }
    public ServiceTicket ServiceTicket { get; set; } = null!;

    public int? ServiceTypeId { get; set; }
    public ServiceType? ServiceType { get; set; }

    public int? PartId { get; set; }
    public Part? Part { get; set; }
}
