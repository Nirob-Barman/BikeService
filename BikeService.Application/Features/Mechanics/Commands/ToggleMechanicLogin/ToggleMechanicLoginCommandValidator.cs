using FluentValidation;

namespace BikeService.Application.Features.Mechanics.Commands.ToggleMechanicLogin;

public class ToggleMechanicLoginCommandValidator : AbstractValidator<ToggleMechanicLoginCommand>
{
    public ToggleMechanicLoginCommandValidator()
    {
        RuleFor(x => x.MechanicId).GreaterThan(0);
    }
}
