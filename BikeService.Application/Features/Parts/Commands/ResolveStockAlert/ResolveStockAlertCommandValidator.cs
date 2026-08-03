using FluentValidation;

namespace BikeService.Application.Features.Parts.Commands.ResolveStockAlert;

public class ResolveStockAlertCommandValidator : AbstractValidator<ResolveStockAlertCommand>
{
    public ResolveStockAlertCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
