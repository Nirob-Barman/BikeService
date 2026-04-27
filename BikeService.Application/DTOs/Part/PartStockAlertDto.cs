namespace BikeService.Application.DTOs.Part
{
    public class PartStockAlertDto
    {
        public int Id { get; set; }
        public int PartId { get; set; }
        public string PartName { get; set; } = string.Empty;
        public string PartSKU { get; set; } = string.Empty;
        public int StockQuantity { get; set; }
        public int LowStockThreshold { get; set; }
        public bool IsResolved { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
