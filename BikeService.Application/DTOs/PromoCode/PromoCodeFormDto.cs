using System.ComponentModel.DataAnnotations;

namespace BikeService.Application.DTOs.PromoCode
{
    public class PromoCodeFormDto
    {
        [Required]
        [MaxLength(50)]
        public string Code { get; set; } = string.Empty;

        [Range(0, 100)]
        public decimal DiscountPercent { get; set; }

        [Range(1, int.MaxValue)]
        public int MaxUsages { get; set; } = 1;

        public DateTime? ExpiresAt { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
