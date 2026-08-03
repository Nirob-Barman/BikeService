using FluentValidation;

namespace BikeService.Application.Features.PaymentGateways.Commands.CreatePaymentGateway;

public class CreatePaymentGatewayCommandValidator : AbstractValidator<CreatePaymentGatewayCommand>
{
    public CreatePaymentGatewayCommandValidator()
    {
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}
