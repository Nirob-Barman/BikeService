using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.Payroll.Commands.CreatePayrollRecord;

public record CreatePayrollRecordCommand(
    int MechanicId,
    int Month,
    int Year,
    decimal BaseSalary,
    decimal Bonus,
    decimal Deductions,
    string? Notes) : IRequest<Result<int>>;
