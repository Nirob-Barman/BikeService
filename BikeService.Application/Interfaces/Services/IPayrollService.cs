using BikeService.Application.DTOs.Payroll;
using BikeService.Application.Wrappers;

namespace BikeService.Application.Interfaces.Services
{
    public interface IPayrollService
    {
        Task<Result<List<PayrollRecordDto>>> GetAllAsync(int? year = null);
        Task<Result<List<PayrollRecordDto>>> GetByMechanicAsync(int mechanicId);
        Task<Result<List<PayrollRecordDto>>> GetMyPayrollAsync();
        Task<Result<PayrollRecordDto>> GetByIdAsync(int id);
        Task<Result<int>> CreateAsync(PayrollRecordFormDto dto);
        Task<Result<bool>> UpdateAsync(int id, PayrollRecordFormDto dto);
        Task<Result<bool>> FinalizeAsync(int id);
        Task<Result<bool>> MarkPaidAsync(int id);
        Task<Result<bool>> DeleteAsync(int id);
    }
}
