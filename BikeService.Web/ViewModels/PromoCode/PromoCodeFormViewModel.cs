using System.ComponentModel.DataAnnotations;

namespace BikeService.Web.ViewModels.PromoCode
{
    public class PromoCodeFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Code is required.")]
        [MaxLength(50, ErrorMessage = "Code cannot exceed 50 characters.")]
        [Display(Name = "Promo Code")]
        public string Code { get; set; } = string.Empty;

        [Required(ErrorMessage = "Discount percent is required.")]
        [Range(0.01, 100.0, ErrorMessage = "Discount must be between 0.01 and 100.")]
        [Display(Name = "Discount (%)")]
        public decimal DiscountPercent { get; set; }

        [Required(ErrorMessage = "Max usages is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Max usages must be at least 1.")]
        [Display(Name = "Max Usages")]
        public int MaxUsages { get; set; } = 1;

        [Display(Name = "Expires At")]
        public DateTime? ExpiresAt { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
    }
}
