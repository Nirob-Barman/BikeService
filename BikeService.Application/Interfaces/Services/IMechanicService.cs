using BikeService.Application.DTOs.Mechanic;
using BikeService.Application.Wrappers;

namespace BikeService.Application.Interfaces.Services
{
    public interface IMechanicService
    {
        Task<Result<List<MechanicDto>>> GetAllAsync();
        Task<Result<MechanicDto>> GetByIdAsync(int id);
        Task<Result<List<MechanicDto>>> GetAvailableAsync();
        Task<Result<int>> CreateAsync(MechanicFormDto dto);
        Task<Result<bool>> UpdateAsync(int id, MechanicFormDto dto);
        Task<Result<bool>> ToggleAvailabilityAsync(int id);
        Task<Result<bool>> DeleteAsync(int id);
        Task<Result<bool>> CreateLoginAsync(int mechanicId, string email, string password);
        Task<Result<bool>> ToggleLoginAsync(int mechanicId);
    }
}
