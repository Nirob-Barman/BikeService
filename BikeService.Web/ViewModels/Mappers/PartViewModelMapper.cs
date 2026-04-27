using BikeService.Application.DTOs.Part;
using BikeService.Web.ViewModels.Inventory;

namespace BikeService.Web.ViewModels.Mappers
{
    public static class PartViewModelMapper
    {
        public static PartFormViewModel ToViewModel(PartDto dto)
            => new PartFormViewModel
            {
                Id = dto.Id,
                Name = dto.Name,
                SKU = dto.SKU,
                UnitPrice = dto.UnitPrice,
                StockQuantity = dto.StockQuantity,
                LowStockThreshold = dto.LowStockThreshold,
            };

        public static PartFormDto ToDto(PartFormViewModel vm)
            => new PartFormDto
            {
                Name = vm.Name,
                SKU = vm.SKU,
                UnitPrice = vm.UnitPrice,
                StockQuantity = vm.StockQuantity,
                LowStockThreshold = vm.LowStockThreshold,
            };
    }
}
