using FluentValidation;

namespace BikeService.Application.Features.Customers.Queries.GetCustomerById;

public class GetCustomerByIdQueryValidator : AbstractValidator<GetCustomerByIdQuery>
{
    public GetCustomerByIdQueryValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
