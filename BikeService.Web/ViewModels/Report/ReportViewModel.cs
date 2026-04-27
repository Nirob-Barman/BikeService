using BikeService.Application.DTOs.Report;

namespace BikeService.Web.ViewModels.Report
{
    public class ReportViewModel
    {
        public DateTime DateFrom { get; set; } = DateTime.Today.AddMonths(-1);
        public DateTime DateTo { get; set; } = DateTime.Today;

        public RevenueReportDto? Revenue { get; set; }
        public TicketReportDto? Tickets { get; set; }
        public List<PartUsageReportDto> PartUsage { get; set; } = new();

        public bool HasData => Revenue != null;
    }
}
