using FluentValidation;

namespace BikeService.Application.Features.Mechanics.Commands.UpdateMechanic;

public class UpdateMechanicCommandValidator : AbstractValidator<UpdateMechanicCommand>
{
    public UpdateMechanicCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(100);
    }
}
