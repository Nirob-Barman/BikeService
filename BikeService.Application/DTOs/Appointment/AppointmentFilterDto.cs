using BikeService.Domain.Enums;

namespace BikeService.Application.DTOs.Appointment
{
    public class AppointmentFilterDto
    {
        public AppointmentStatus? Status { get; set; }
        public string? CustomerId { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
    }
}
