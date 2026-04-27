namespace BikeService.Application.DTOs.Dashboard
{
    public class DashboardDto
    {
        public decimal TotalRevenue { get; set; }
        public decimal RevenueThisMonth { get; set; }
        public int TicketsToday { get; set; }
        public int PendingTickets { get; set; }
        public int ActiveTickets { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalBikes { get; set; }
        public List<ServiceStatDto> TopServices { get; set; } = new();
        public List<MechanicStatDto> TopMechanics { get; set; } = new();
        public List<MonthlyRevenueDto> MonthlyRevenue { get; set; } = new();
    }

    public class ServiceStatDto
    {
        public string ServiceName { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal Revenue { get; set; }
    }

    public class MechanicStatDto
    {
        public string MechanicName { get; set; } = string.Empty;
        public int TicketsCompleted { get; set; }
    }

    public class MonthlyRevenueDto
    {
        public string Month { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
    }
}
