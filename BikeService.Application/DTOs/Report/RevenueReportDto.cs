namespace BikeService.Application.DTOs.Report
{
    public class RevenueReportDto
    {
        public decimal TotalRevenue { get; set; }
        public decimal TotalTax { get; set; }
        public decimal TotalDiscount { get; set; }
        public int InvoiceCount { get; set; }
        public List<RevenueByServiceDto> ByService { get; set; } = new();
        public List<RevenueByMechanicDto> ByMechanic { get; set; } = new();
    }

    public class RevenueByServiceDto
    {
        public string ServiceName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Revenue { get; set; }
    }

    public class RevenueByMechanicDto
    {
        public string MechanicName { get; set; } = string.Empty;
        public int TicketsDelivered { get; set; }
        public decimal Revenue { get; set; }
    }
}
