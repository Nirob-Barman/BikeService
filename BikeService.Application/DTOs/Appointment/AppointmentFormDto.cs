namespace BikeService.Application.DTOs.Appointment
{
    public class AppointmentFormDto
    {
        public DateTime AppointmentDate { get; set; }
        public string? Notes { get; set; }
        public int BikeId { get; set; }
    }
}
