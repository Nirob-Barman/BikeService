using BikeService.Application.DTOs.Payroll;
using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.Payroll.Queries.GetPayrollRecordsByMechanic;

public record GetPayrollRecordsByMechanicQuery(int MechanicId) : IRequest<Result<List<PayrollRecordDto>>>;
