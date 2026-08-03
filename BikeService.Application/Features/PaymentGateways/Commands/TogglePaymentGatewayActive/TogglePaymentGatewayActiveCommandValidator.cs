using FluentValidation;

namespace BikeService.Application.Features.PaymentGateways.Commands.TogglePaymentGatewayActive;

public class TogglePaymentGatewayActiveCommandValidator : AbstractValidator<TogglePaymentGatewayActiveCommand>
{
    public TogglePaymentGatewayActiveCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
