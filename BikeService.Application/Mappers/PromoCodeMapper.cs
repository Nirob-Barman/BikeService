using BikeService.Application.DTOs.PromoCode;
using BikeService.Domain.Entities;

namespace BikeService.Application.Mappers
{
    public static class PromoCodeMapper
    {
        public static PromoCodeDto ToDto(PromoCode e) => new()
        {
            Id = e.Id,
            Code = e.Code,
            DiscountPercent = e.DiscountPercent,
            MaxUsages = e.MaxUsages,
            UsageCount = e.UsageCount,
            ExpiresAt = e.ExpiresAt,
            IsActive = e.IsActive,
            CreatedAt = e.CreatedAt
        };

        public static PromoCode ToEntity(PromoCodeFormDto dto) => new()
        {
            Code = dto.Code,
            DiscountPercent = dto.DiscountPercent,
            MaxUsages = dto.MaxUsages,
            ExpiresAt = dto.ExpiresAt,
            IsActive = dto.IsActive
        };

        public static void UpdateEntity(PromoCode e, PromoCodeFormDto dto)
        {
            e.Code = dto.Code;
            e.DiscountPercent = dto.DiscountPercent;
            e.MaxUsages = dto.MaxUsages;
            e.ExpiresAt = dto.ExpiresAt;
            e.IsActive = dto.IsActive;
        }
    }
}
