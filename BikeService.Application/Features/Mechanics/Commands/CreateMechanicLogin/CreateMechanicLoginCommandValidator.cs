using FluentValidation;

namespace BikeService.Application.Features.Mechanics.Commands.CreateMechanicLogin;

public class CreateMechanicLoginCommandValidator : AbstractValidator<CreateMechanicLoginCommand>
{
    public CreateMechanicLoginCommandValidator()
    {
        RuleFor(x => x.MechanicId).GreaterThan(0);
        RuleFor(x => x.Email).NotEmpty();
        RuleFor(x => x.Password).NotEmpty();
    }
}
