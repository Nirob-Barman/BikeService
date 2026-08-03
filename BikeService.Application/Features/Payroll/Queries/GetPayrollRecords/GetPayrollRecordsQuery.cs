using BikeService.Application.DTOs.Payroll;
using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.Payroll.Queries.GetPayrollRecords;

public record GetPayrollRecordsQuery(int? Year = null) : IRequest<Result<List<PayrollRecordDto>>>;
