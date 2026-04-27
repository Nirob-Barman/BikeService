using BikeService.Application.DTOs.LeaveRequest;
using BikeService.Application.Wrappers;

namespace BikeService.Application.Interfaces.Services
{
    public interface ILeaveRequestService
    {
        Task<Result<List<LeaveRequestDto>>> GetAllAsync();
        Task<Result<List<LeaveRequestDto>>> GetByMechanicAsync(int mechanicId);
        Task<Result<List<LeaveRequestDto>>> GetMyLeaveRequestsAsync();
        Task<Result<LeaveRequestDto>> GetByIdAsync(int id);
        Task<Result<int>> SubmitAsync(LeaveRequestFormDto dto);
        Task<Result<bool>> ApproveAsync(int id, string? adminNotes);
        Task<Result<bool>> RejectAsync(int id, string? adminNotes);
        Task<Result<bool>> CancelAsync(int id);
    }
}
