using FluentValidation;

namespace BikeService.Application.Features.Payroll.Commands.FinalizePayrollRecord;

public class FinalizePayrollRecordCommandValidator : AbstractValidator<FinalizePayrollRecordCommand>
{
    public FinalizePayrollRecordCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
