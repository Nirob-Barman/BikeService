using FluentValidation;

namespace BikeService.Application.Features.PromoCodes.Commands.DeletePromoCode;

public class DeletePromoCodeCommandValidator : AbstractValidator<DeletePromoCodeCommand>
{
    public DeletePromoCodeCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
