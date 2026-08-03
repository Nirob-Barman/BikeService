using FluentValidation;

namespace BikeService.Application.Features.CustomerBikes.Commands.CreateCustomerBike;

public class CreateCustomerBikeCommandValidator : AbstractValidator<CreateCustomerBikeCommand>
{
    public CreateCustomerBikeCommandValidator()
    {
        RuleFor(x => x.Make).NotEmpty();
        RuleFor(x => x.Model).NotEmpty();
        RuleFor(x => x.Year).InclusiveBetween(1900, 2100).WithMessage("Year must be between 1900 and 2100.");
    }
}
