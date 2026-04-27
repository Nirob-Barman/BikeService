using BikeService.Domain.Enums;

namespace BikeService.Application.DTOs.Appointment
{
    public class AppointmentDto
    {
        public int Id { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string? Notes { get; set; }
        public AppointmentStatus Status { get; set; }
        public string StatusLabel => Status.ToString();
        public int BikeId { get; set; }
        public string BikeSummary { get; set; } = string.Empty;
        public string CustomerId { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool HasTicket { get; set; }
    }
}
