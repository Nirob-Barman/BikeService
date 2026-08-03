using FluentValidation;

namespace BikeService.Application.Features.Mechanics.Commands.CreateMechanic;

public class CreateMechanicCommandValidator : AbstractValidator<CreateMechanicCommand>
{
    public CreateMechanicCommandValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(100);
    }
}
