using FluentValidation;

namespace BikeService.Application.Features.PaymentGateways.Commands.DeletePaymentGateway;

public class DeletePaymentGatewayCommandValidator : AbstractValidator<DeletePaymentGatewayCommand>
{
    public DeletePaymentGatewayCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
