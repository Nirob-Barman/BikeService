using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.Payroll.Commands.MarkPayrollRecordPaid;

public record MarkPayrollRecordPaidCommand(int Id) : IRequest<Result<bool>>;
