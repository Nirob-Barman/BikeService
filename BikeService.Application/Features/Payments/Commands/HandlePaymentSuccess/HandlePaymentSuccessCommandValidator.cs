using FluentValidation;

namespace BikeService.Application.Features.Payments.Commands.HandlePaymentSuccess;

public class HandlePaymentSuccessCommandValidator : AbstractValidator<HandlePaymentSuccessCommand>
{
    public HandlePaymentSuccessCommandValidator()
    {
        RuleFor(x => x.TxId).GreaterThan(0);
    }
}
