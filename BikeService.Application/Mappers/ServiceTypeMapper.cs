using BikeService.Application.DTOs.ServiceType;
using BikeService.Domain.Entities;

namespace BikeService.Application.Mappers
{
    public static class ServiceTypeMapper
    {
        public static ServiceTypeDto ToDto(ServiceType e) => new()
        {
            Id = e.Id,
            Name = e.Name,
            Description = e.Description,
            BasePrice = e.BasePrice,
            EstimatedHours = e.EstimatedHours,
            IsActive = e.IsActive,
            CreatedAt = e.CreatedAt
        };

        public static ServiceType ToEntity(ServiceTypeFormDto dto) => new()
        {
            Name = dto.Name,
            Description = dto.Description,
            BasePrice = dto.BasePrice,
            EstimatedHours = dto.EstimatedHours,
            IsActive = dto.IsActive
        };

        public static void UpdateEntity(ServiceType e, ServiceTypeFormDto dto)
        {
            e.Name = dto.Name;
            e.Description = dto.Description;
            e.BasePrice = dto.BasePrice;
            e.EstimatedHours = dto.EstimatedHours;
            e.IsActive = dto.IsActive;
        }
    }
}
