using FluentValidation;

namespace BikeService.Application.Features.PaymentGateways.Commands.UpdatePaymentGateway;

public class UpdatePaymentGatewayCommandValidator : AbstractValidator<UpdatePaymentGatewayCommand>
{
    public UpdatePaymentGatewayCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}
