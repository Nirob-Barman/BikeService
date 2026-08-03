using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.Payroll.Commands.UpdatePayrollRecord;

public record UpdatePayrollRecordCommand(
    int Id,
    int MechanicId,
    int Month,
    int Year,
    decimal BaseSalary,
    decimal Bonus,
    decimal Deductions,
    string? Notes) : IRequest<Result<bool>>;
