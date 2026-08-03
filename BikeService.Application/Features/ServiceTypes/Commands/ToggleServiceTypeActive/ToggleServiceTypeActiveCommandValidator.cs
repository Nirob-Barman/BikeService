using FluentValidation;

namespace BikeService.Application.Features.ServiceTypes.Commands.ToggleServiceTypeActive;

public class ToggleServiceTypeActiveCommandValidator : AbstractValidator<ToggleServiceTypeActiveCommand>
{
    public ToggleServiceTypeActiveCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
