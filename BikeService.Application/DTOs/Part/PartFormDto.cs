using System.ComponentModel.DataAnnotations;

namespace BikeService.Application.DTOs.Part
{
    public class PartFormDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string SKU { get; set; } = string.Empty;

        [Range(0.01, double.MaxValue, ErrorMessage = "Unit price must be greater than 0.")]
        public decimal UnitPrice { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Stock quantity cannot be negative.")]
        public int StockQuantity { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Low stock threshold cannot be negative.")]
        public int LowStockThreshold { get; set; }
    }
}
