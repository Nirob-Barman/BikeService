using FluentValidation;

namespace BikeService.Application.Features.PromoCodes.Commands.UpdatePromoCode;

public class UpdatePromoCodeCommandValidator : AbstractValidator<UpdatePromoCodeCommand>
{
    public UpdatePromoCodeCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.DiscountPercent).InclusiveBetween(0, 100);
        RuleFor(x => x.MaxUsages).GreaterThan(0);
    }
}
