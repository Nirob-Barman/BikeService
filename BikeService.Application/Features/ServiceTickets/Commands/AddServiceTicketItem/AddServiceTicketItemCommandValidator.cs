using FluentValidation;

namespace BikeService.Application.Features.ServiceTickets.Commands.AddServiceTicketItem;

public class AddServiceTicketItemCommandValidator : AbstractValidator<AddServiceTicketItemCommand>
{
    public AddServiceTicketItemCommandValidator()
    {
        RuleFor(x => x.TicketId).GreaterThan(0);
        RuleFor(x => x)
            .Must(x => x.ServiceTypeId.HasValue || x.PartId.HasValue)
            .WithMessage("Either a service type or a part must be specified.")
            .WithName("ServiceTypeId");
        RuleFor(x => x.Quantity).GreaterThanOrEqualTo(1).WithMessage("Quantity must be at least 1.");
    }
}
