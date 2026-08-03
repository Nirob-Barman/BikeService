using FluentValidation;

namespace BikeService.Application.Features.Customers.Commands.BanCustomer;

public class BanCustomerCommandValidator : AbstractValidator<BanCustomerCommand>
{
    public BanCustomerCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
