using BikeService.Application.DTOs.Appointment;
using BikeService.Application.Wrappers;

namespace BikeService.Application.Interfaces.Services
{
    public interface IAppointmentService
    {
        Task<Result<List<AppointmentDto>>> GetAllAsync(AppointmentFilterDto? filter = null);
        Task<Result<AppointmentDto>> GetByIdAsync(int id);
        Task<Result<List<AppointmentDto>>> GetMyAppointmentsAsync();
        Task<Result<int>> CreateAsync(AppointmentFormDto dto);
        Task<Result<bool>> ConfirmAsync(int id);
        Task<Result<bool>> CancelAsync(int id);
        Task<Result<bool>> CompleteAsync(int id);
    }
}
