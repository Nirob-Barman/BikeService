using FluentValidation;

namespace BikeService.Application.Features.Payments.Commands.HandlePaymentCancel;

public class HandlePaymentCancelCommandValidator : AbstractValidator<HandlePaymentCancelCommand>
{
    public HandlePaymentCancelCommandValidator()
    {
        RuleFor(x => x.TxId).GreaterThan(0);
    }
}
