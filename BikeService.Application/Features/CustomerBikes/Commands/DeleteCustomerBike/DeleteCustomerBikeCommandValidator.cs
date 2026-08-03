using FluentValidation;

namespace BikeService.Application.Features.CustomerBikes.Commands.DeleteCustomerBike;

public class DeleteCustomerBikeCommandValidator : AbstractValidator<DeleteCustomerBikeCommand>
{
    public DeleteCustomerBikeCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
