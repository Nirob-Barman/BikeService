using FluentValidation;

namespace BikeService.Application.Features.Payments.Commands.InitiatePayment;

public class InitiatePaymentCommandValidator : AbstractValidator<InitiatePaymentCommand>
{
    public InitiatePaymentCommandValidator()
    {
        RuleFor(x => x.InvoiceId).GreaterThan(0);
        RuleFor(x => x.GatewayId).GreaterThan(0);
    }
}
