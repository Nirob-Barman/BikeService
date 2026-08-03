using FluentValidation;

namespace BikeService.Application.Features.ServiceTickets.Commands.RemoveServiceTicketItem;

public class RemoveServiceTicketItemCommandValidator : AbstractValidator<RemoveServiceTicketItemCommand>
{
    public RemoveServiceTicketItemCommandValidator()
    {
        RuleFor(x => x.ItemId).GreaterThan(0);
    }
}
