using FluentValidation;

namespace BikeService.Application.Features.Parts.Commands.UpdatePart;

public class UpdatePartCommandValidator : AbstractValidator<UpdatePartCommand>
{
    public UpdatePartCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.SKU).NotEmpty().MaximumLength(50);
        RuleFor(x => x.UnitPrice).GreaterThan(0);
        RuleFor(x => x.StockQuantity).GreaterThanOrEqualTo(0);
        RuleFor(x => x.LowStockThreshold).GreaterThanOrEqualTo(0);
    }
}
