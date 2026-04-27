using BikeService.Application.DTOs.PromoCode;
using BikeService.Web.ViewModels.PromoCode;

namespace BikeService.Web.ViewModels.Mappers
{
    public static class PromoCodeViewModelMapper
    {
        public static PromoCodeFormViewModel ToViewModel(PromoCodeDto dto)
            => new PromoCodeFormViewModel
            {
                Id = dto.Id,
                Code = dto.Code,
                DiscountPercent = dto.DiscountPercent,
                MaxUsages = dto.MaxUsages,
                ExpiresAt = dto.ExpiresAt,
                IsActive = dto.IsActive,
            };

        public static PromoCodeFormDto ToDto(PromoCodeFormViewModel vm)
            => new PromoCodeFormDto
            {
                Code = vm.Code,
                DiscountPercent = vm.DiscountPercent,
                MaxUsages = vm.MaxUsages,
                ExpiresAt = vm.ExpiresAt,
                IsActive = vm.IsActive,
            };
    }
}
