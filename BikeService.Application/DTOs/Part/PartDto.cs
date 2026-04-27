namespace BikeService.Application.DTOs.Part
{
    public class PartDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int StockQuantity { get; set; }
        public int LowStockThreshold { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsLowStock => StockQuantity <= LowStockThreshold;
    }
}
