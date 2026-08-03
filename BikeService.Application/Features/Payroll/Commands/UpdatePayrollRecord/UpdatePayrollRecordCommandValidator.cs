using FluentValidation;

namespace BikeService.Application.Features.Payroll.Commands.UpdatePayrollRecord;

public class UpdatePayrollRecordCommandValidator : AbstractValidator<UpdatePayrollRecordCommand>
{
    public UpdatePayrollRecordCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.MechanicId).GreaterThan(0);
        RuleFor(x => x.Month).InclusiveBetween(1, 12);
        RuleFor(x => x.Year).InclusiveBetween(2000, 2100);
    }
}
