namespace BikeService.Domain.Entities;

public class CustomerBike : BaseEntity
{
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public string? RegistrationNo { get; set; }
    public string? ImageUrl { get; set; }

    public string CustomerId { get; set; } = string.Empty;

    public ICollection<Appointment> Appointments { get; set; } = [];
    public ICollection<ServiceTicket> ServiceTickets { get; set; } = [];
}
