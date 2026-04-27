using BikeService.Domain.Enums;

namespace BikeService.Application.DTOs.ServiceTicket
{
    public class ServiceTicketDto
    {
        public int Id { get; set; }
        public ServiceTicketStatus Status { get; set; }
        public string StatusLabel => Status.ToString();
        public string? DiagnosisNotes { get; set; }
        public DateTime? EstimatedCompletionDate { get; set; }

        public int BikeId { get; set; }
        public string BikeSummary { get; set; } = string.Empty;

        public int? MechanicId { get; set; }
        public string? MechanicName { get; set; }

        public string CustomerId { get; set; } = string.Empty;
        public string? CustomerName { get; set; }

        public int? AppointmentId { get; set; }
        public DateTime CreatedAt { get; set; }

        public bool IsOverdue =>
            EstimatedCompletionDate.HasValue &&
            EstimatedCompletionDate.Value < DateTime.UtcNow &&
            Status != ServiceTicketStatus.Delivered &&
            Status != ServiceTicketStatus.Cancelled;

        public List<ServiceTicketItemDto> Items { get; set; } = new();
        public bool HasInvoice { get; set; }
        public int? InvoiceId { get; set; }
    }
}
