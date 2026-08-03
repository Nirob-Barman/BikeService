using FluentValidation;

namespace BikeService.Application.Features.Mechanics.Commands.ToggleMechanicAvailability;

public class ToggleMechanicAvailabilityCommandValidator : AbstractValidator<ToggleMechanicAvailabilityCommand>
{
    public ToggleMechanicAvailabilityCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
