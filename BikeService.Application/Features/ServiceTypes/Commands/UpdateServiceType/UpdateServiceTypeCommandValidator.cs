using FluentValidation;

namespace BikeService.Application.Features.ServiceTypes.Commands.UpdateServiceType;

public class UpdateServiceTypeCommandValidator : AbstractValidator<UpdateServiceTypeCommand>
{
    public UpdateServiceTypeCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.BasePrice).InclusiveBetween(0.01m, 999999.99m);
        RuleFor(x => x.EstimatedHours).InclusiveBetween(0.1, 999.0);
    }
}
