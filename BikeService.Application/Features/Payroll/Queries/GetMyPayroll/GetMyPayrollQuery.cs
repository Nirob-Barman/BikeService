using BikeService.Application.DTOs.Payroll;
using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.Payroll.Queries.GetMyPayroll;

public record GetMyPayrollQuery : IRequest<Result<List<PayrollRecordDto>>>;
