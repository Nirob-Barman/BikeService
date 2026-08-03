using FluentValidation;

namespace BikeService.Application.Features.ServiceTypes.Commands.CreateServiceType;

public class CreateServiceTypeCommandValidator : AbstractValidator<CreateServiceTypeCommand>
{
    public CreateServiceTypeCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.BasePrice).InclusiveBetween(0.01m, 999999.99m);
        RuleFor(x => x.EstimatedHours).InclusiveBetween(0.1, 999.0);
    }
}
