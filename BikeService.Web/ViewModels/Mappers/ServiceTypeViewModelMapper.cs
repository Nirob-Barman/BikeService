using BikeService.Application.DTOs.ServiceType;
using BikeService.Web.ViewModels.ServiceType;

namespace BikeService.Web.ViewModels.Mappers
{
    public static class ServiceTypeViewModelMapper
    {
        public static ServiceTypeFormViewModel ToViewModel(ServiceTypeDto dto)
            => new ServiceTypeFormViewModel
            {
                Id = dto.Id,
                Name = dto.Name,
                Description = dto.Description,
                BasePrice = dto.BasePrice,
                EstimatedHours = dto.EstimatedHours,
                IsActive = dto.IsActive,
            };

        public static ServiceTypeFormDto ToDto(ServiceTypeFormViewModel vm)
            => new ServiceTypeFormDto
            {
                Name = vm.Name,
                Description = vm.Description,
                BasePrice = vm.BasePrice,
                EstimatedHours = vm.EstimatedHours,
                IsActive = vm.IsActive,
            };
    }
}
