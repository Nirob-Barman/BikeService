using BikeService.Application.DTOs.CustomerBike;
using BikeService.Application.Wrappers;

namespace BikeService.Application.Interfaces.Services
{
    public interface ICustomerBikeService
    {
        Task<Result<List<CustomerBikeDto>>> GetAllAsync();
        Task<Result<List<CustomerBikeDto>>> GetMyBikesAsync();
        Task<Result<CustomerBikeDto>> GetByIdAsync(int id);
        Task<Result<int>> CreateAsync(CustomerBikeFormDto dto);
        Task<Result<bool>> UpdateAsync(int id, CustomerBikeFormDto dto);
        Task<Result<bool>> DeleteAsync(int id);
    }
}
