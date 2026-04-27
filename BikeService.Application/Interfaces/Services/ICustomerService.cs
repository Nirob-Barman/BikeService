using BikeService.Application.DTOs.Customer;
using BikeService.Application.Wrappers;

namespace BikeService.Application.Interfaces.Services
{
    public interface ICustomerService
    {
        Task<Result<List<CustomerDto>>> GetAllAsync();
        Task<Result<CustomerDto>> GetByIdAsync(string id);
        Task<Result<bool>> BanAsync(string id);
        Task<Result<bool>> UnbanAsync(string id);
    }
}
