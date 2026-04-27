using BikeService.Application.DTOs.ServiceType;
using BikeService.Application.Wrappers;

namespace BikeService.Application.Interfaces.Services
{
    public interface IServiceTypeService
    {
        Task<Result<List<ServiceTypeDto>>> GetAllAsync();
        Task<Result<ServiceTypeDto>> GetByIdAsync(int id);
        Task<Result<List<ServiceTypeDto>>> GetActiveAsync();
        Task<Result<int>> CreateAsync(ServiceTypeFormDto dto);
        Task<Result<bool>> UpdateAsync(int id, ServiceTypeFormDto dto);
        Task<Result<bool>> ToggleActiveAsync(int id);
        Task<Result<bool>> DeleteAsync(int id);
    }
}
