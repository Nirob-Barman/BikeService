using System.ComponentModel.DataAnnotations;

namespace BikeService.Web.ViewModels.Inventory
{
    public class PartFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Part name is required.")]
        [MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
        [Display(Name = "Part Name")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "SKU is required.")]
        [MaxLength(50, ErrorMessage = "SKU cannot exceed 50 characters.")]
        [Display(Name = "SKU")]
        public string SKU { get; set; } = string.Empty;

        [Required(ErrorMessage = "Unit price is required.")]
        [Range(0.01, 999999.99, ErrorMessage = "Unit price must be greater than 0.")]
        [Display(Name = "Unit Price ($)")]
        public decimal UnitPrice { get; set; }

        [Required(ErrorMessage = "Stock quantity is required.")]
        [Range(0, int.MaxValue, ErrorMessage = "Stock quantity cannot be negative.")]
        [Display(Name = "Stock Quantity")]
        public int StockQuantity { get; set; }

        [Required(ErrorMessage = "Low stock threshold is required.")]
        [Range(0, int.MaxValue, ErrorMessage = "Threshold cannot be negative.")]
        [Display(Name = "Low Stock Threshold")]
        public int LowStockThreshold { get; set; }
    }
}
