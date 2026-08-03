using FluentValidation;

namespace BikeService.Application.Features.Customers.Commands.UnbanCustomer;

public class UnbanCustomerCommandValidator : AbstractValidator<UnbanCustomerCommand>
{
    public UnbanCustomerCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
