namespace BikeService.Application.DTOs.PromoCode
{
    public class PromoCodeDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public decimal DiscountPercent { get; set; }
        public int MaxUsages { get; set; }
        public int UsageCount { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }

        public int RemainingUsages => MaxUsages - UsageCount;
        public bool IsExpired => ExpiresAt.HasValue && ExpiresAt.Value < DateTime.UtcNow;
    }
}
