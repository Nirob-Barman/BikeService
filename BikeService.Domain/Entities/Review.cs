namespace BikeService.Domain.Entities;

public class Review : BaseEntity
{
    public int Rating { get; set; }
    public string? Comment { get; set; }

    public int ServiceTicketId { get; set; }
    public ServiceTicket ServiceTicket { get; set; } = null!;

    public string CustomerId { get; set; } = string.Empty;
}
