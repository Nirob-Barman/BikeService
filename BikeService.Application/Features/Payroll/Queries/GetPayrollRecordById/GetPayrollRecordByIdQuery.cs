using BikeService.Application.DTOs.Payroll;
using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.Payroll.Queries.GetPayrollRecordById;

public record GetPayrollRecordByIdQuery(int Id) : IRequest<Result<PayrollRecordDto>>;
