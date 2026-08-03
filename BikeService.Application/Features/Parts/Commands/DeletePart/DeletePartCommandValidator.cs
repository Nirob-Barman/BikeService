using FluentValidation;

namespace BikeService.Application.Features.Parts.Commands.DeletePart;

public class DeletePartCommandValidator : AbstractValidator<DeletePartCommand>
{
    public DeletePartCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
