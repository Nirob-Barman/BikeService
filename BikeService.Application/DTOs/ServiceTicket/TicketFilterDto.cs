using BikeService.Domain.Enums;

namespace BikeService.Application.DTOs.ServiceTicket
{
    public class TicketFilterDto
    {
        public ServiceTicketStatus? Status { get; set; }
        public int? MechanicId { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public string? CustomerId { get; set; }
    }
}
