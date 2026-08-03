using FluentValidation;

namespace BikeService.Application.Features.PromoCodes.Commands.CreatePromoCode;

public class CreatePromoCodeCommandValidator : AbstractValidator<CreatePromoCodeCommand>
{
    public CreatePromoCodeCommandValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.DiscountPercent).InclusiveBetween(0, 100);
        RuleFor(x => x.MaxUsages).GreaterThan(0);
    }
}
