using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.Payroll.Commands.DeletePayrollRecord;

public record DeletePayrollRecordCommand(int Id) : IRequest<Result<bool>>;
