using FluentValidation;

namespace BikeService.Application.Features.Payroll.Commands.CreatePayrollRecord;

public class CreatePayrollRecordCommandValidator : AbstractValidator<CreatePayrollRecordCommand>
{
    public CreatePayrollRecordCommandValidator()
    {
        RuleFor(x => x.MechanicId).GreaterThan(0);
        RuleFor(x => x.Month).InclusiveBetween(1, 12);
        RuleFor(x => x.Year).InclusiveBetween(2000, 2100);
    }
}
