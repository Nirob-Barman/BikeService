using BikeService.Application.DTOs.Payroll;
using BikeService.Domain.Entities;

namespace BikeService.Application.Mappers
{
    public static class PayrollRecordMapper
    {
        public static PayrollRecordDto ToDto(PayrollRecord e) => new()
        {
            Id           = e.Id,
            MechanicId   = e.MechanicId,
            MechanicName = e.Mechanic?.FullName ?? string.Empty,
            Month        = e.Month,
            Year         = e.Year,
            BaseSalary   = e.BaseSalary,
            Bonus        = e.Bonus,
            Deductions   = e.Deductions,
            Status       = e.Status,
            Notes        = e.Notes,
            CreatedAt    = e.CreatedAt,
        };
    }
}
