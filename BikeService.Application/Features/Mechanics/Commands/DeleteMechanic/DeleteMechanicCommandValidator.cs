using FluentValidation;

namespace BikeService.Application.Features.Mechanics.Commands.DeleteMechanic;

public class DeleteMechanicCommandValidator : AbstractValidator<DeleteMechanicCommand>
{
    public DeleteMechanicCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
