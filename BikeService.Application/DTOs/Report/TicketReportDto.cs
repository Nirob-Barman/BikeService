namespace BikeService.Application.DTOs.Report
{
    public class TicketReportDto
    {
        public int Total { get; set; }
        public int Delivered { get; set; }
        public int Cancelled { get; set; }
        public int Active { get; set; }
        public int Overdue { get; set; }
        public double AvgCompletionDays { get; set; }
        public List<TicketsByStatusDto> ByStatus { get; set; } = new();
        public List<TicketsByMechanicDto> ByMechanic { get; set; } = new();
    }

    public class TicketsByStatusDto
    {
        public string Status { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class TicketsByMechanicDto
    {
        public string MechanicName { get; set; } = string.Empty;
        public int Total { get; set; }
        public int Delivered { get; set; }
    }
}
