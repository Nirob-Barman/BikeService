using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.Payroll.Commands.FinalizePayrollRecord;

public record FinalizePayrollRecordCommand(int Id) : IRequest<Result<bool>>;
