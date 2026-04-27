using BikeService.Domain.Enums;

namespace BikeService.Domain.Entities;

public class ServiceTicket : BaseEntity
{
    public string? DiagnosisNotes { get; set; }
    public DateTime? EstimatedCompletionDate { get; set; }
    public ServiceTicketStatus Status { get; set; } = ServiceTicketStatus.Pending;

    public int BikeId { get; set; }
    public CustomerBike Bike { get; set; } = null!;

    public int? MechanicId { get; set; }
    public Mechanic? Mechanic { get; set; }

    public int? AppointmentId { get; set; }
    public Appointment? Appointment { get; set; }

    public ICollection<ServiceTicketItem> Items { get; set; } = [];
    public ICollection<Invoice> Invoices { get; set; } = [];
    public ICollection<Review> Reviews { get; set; } = [];
    public ICollection<TicketNote> Notes { get; set; } = [];
}
