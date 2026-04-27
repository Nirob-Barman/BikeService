namespace BikeService.Domain.Entities;

public class TicketNote : BaseEntity
{
    public int ServiceTicketId { get; set; }
    public ServiceTicket ServiceTicket { get; set; } = null!;

    public string AuthorId { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public string AuthorRole { get; set; } = string.Empty;   // "Customer" | "Mechanic"
    public string Message { get; set; } = string.Empty;
}
