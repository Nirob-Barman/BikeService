namespace BikeService.Application.DTOs.ServiceTicket
{
    public class ServiceTicketFormDto
    {
        public int BikeId { get; set; }
        public int? MechanicId { get; set; }
        public int? AppointmentId { get; set; }
        public string? DiagnosisNotes { get; set; }
        public DateTime? EstimatedCompletionDate { get; set; }
    }
}
