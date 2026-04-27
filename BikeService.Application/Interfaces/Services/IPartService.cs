using BikeService.Application.DTOs.Part;
using BikeService.Application.Wrappers;

namespace BikeService.Application.Interfaces.Services
{
    public interface IPartService
    {
        Task<Result<List<PartDto>>> GetAllAsync();
        Task<Result<PartDto>> GetByIdAsync(int id);
        Task<Result<int>> CreateAsync(PartFormDto dto);
        Task<Result<bool>> UpdateAsync(int id, PartFormDto dto);
        Task<Result<bool>> DeleteAsync(int id);
        Task<Result<List<PartStockAlertDto>>> GetStockAlertsAsync(bool unresolvedOnly = true);
        Task<Result<bool>> ResolveStockAlertAsync(int alertId);
    }
}
