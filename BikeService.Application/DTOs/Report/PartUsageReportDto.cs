namespace BikeService.Application.DTOs.Report
{
    public class PartUsageReportDto
    {
        public string PartName { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public int TotalQuantity { get; set; }
        public decimal TotalValue { get; set; }
        public int TimesUsed { get; set; }
    }
}
