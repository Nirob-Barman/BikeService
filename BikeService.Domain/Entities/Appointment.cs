using BikeService.Domain.Enums;

namespace BikeService.Domain.Entities;

public class Appointment : BaseEntity
{
    public DateTime AppointmentDate { get; set; }
    public string? Notes { get; set; }
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;

    public int BikeId { get; set; }
    public CustomerBike Bike { get; set; } = null!;

    public string CustomerId { get; set; } = string.Empty;

    public ICollection<ServiceTicket> ServiceTickets { get; set; } = [];
}
