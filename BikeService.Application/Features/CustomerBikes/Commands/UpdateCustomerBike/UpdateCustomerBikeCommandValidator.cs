using FluentValidation;

namespace BikeService.Application.Features.CustomerBikes.Commands.UpdateCustomerBike;

public class UpdateCustomerBikeCommandValidator : AbstractValidator<UpdateCustomerBikeCommand>
{
    public UpdateCustomerBikeCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Make).NotEmpty();
        RuleFor(x => x.Model).NotEmpty();
        RuleFor(x => x.Year).InclusiveBetween(1900, 2100).WithMessage("Year must be between 1900 and 2100.");
    }
}
