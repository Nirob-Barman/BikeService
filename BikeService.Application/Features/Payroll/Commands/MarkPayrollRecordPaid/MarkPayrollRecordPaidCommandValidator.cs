using FluentValidation;

namespace BikeService.Application.Features.Payroll.Commands.MarkPayrollRecordPaid;

public class MarkPayrollRecordPaidCommandValidator : AbstractValidator<MarkPayrollRecordPaidCommand>
{
    public MarkPayrollRecordPaidCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
