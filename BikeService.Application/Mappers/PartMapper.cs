using BikeService.Application.DTOs.Part;
using BikeService.Domain.Entities;

namespace BikeService.Application.Mappers
{
    public static class PartMapper
    {
        public static PartDto ToDto(Part part) => new()
        {
            Id = part.Id,
            Name = part.Name,
            SKU = part.SKU,
            UnitPrice = part.UnitPrice,
            StockQuantity = part.StockQuantity,
            LowStockThreshold = part.LowStockThreshold,
            CreatedAt = part.CreatedAt
        };

        public static Part ToEntity(PartFormDto dto) => new()
        {
            Name = dto.Name,
            SKU = dto.SKU,
            UnitPrice = dto.UnitPrice,
            StockQuantity = dto.StockQuantity,
            LowStockThreshold = dto.LowStockThreshold
        };

        public static void UpdateEntity(Part part, PartFormDto dto)
        {
            part.Name = dto.Name;
            part.SKU = dto.SKU;
            part.UnitPrice = dto.UnitPrice;
            part.StockQuantity = dto.StockQuantity;
            part.LowStockThreshold = dto.LowStockThreshold;
        }

        public static PartStockAlertDto ToStockAlertDto(PartStockAlert alert, Part part) => new()
        {
            Id = alert.Id,
            PartId = alert.PartId,
            PartName = part.Name,
            PartSKU = part.SKU,
            StockQuantity = part.StockQuantity,
            LowStockThreshold = part.LowStockThreshold,
            IsResolved = alert.IsResolved,
            CreatedAt = alert.CreatedAt
        };
    }
}
