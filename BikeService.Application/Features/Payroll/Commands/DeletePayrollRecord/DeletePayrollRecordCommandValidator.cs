using FluentValidation;

namespace BikeService.Application.Features.Payroll.Commands.DeletePayrollRecord;

public class DeletePayrollRecordCommandValidator : AbstractValidator<DeletePayrollRecordCommand>
{
    public DeletePayrollRecordCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
