using BikeService.Application.DTOs.Dashboard;
using BikeService.Application.Wrappers;

namespace BikeService.Application.Interfaces.Services
{
    public interface IDashboardService
    {
        Task<Result<DashboardDto>> GetDashboardAsync();
    }
}
