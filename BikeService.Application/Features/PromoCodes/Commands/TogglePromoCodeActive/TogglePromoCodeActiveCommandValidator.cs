using FluentValidation;

namespace BikeService.Application.Features.PromoCodes.Commands.TogglePromoCodeActive;

public class TogglePromoCodeActiveCommandValidator : AbstractValidator<TogglePromoCodeActiveCommand>
{
    public TogglePromoCodeActiveCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
