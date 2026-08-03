using FluentValidation;

namespace BikeService.Application.Features.ServiceTypes.Commands.DeleteServiceType;

public class DeleteServiceTypeCommandValidator : AbstractValidator<DeleteServiceTypeCommand>
{
    public DeleteServiceTypeCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
