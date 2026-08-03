using FluentValidation;

namespace BikeService.Application.Features.ServiceTickets.Commands.CancelServiceTicket;

public class CancelServiceTicketCommandValidator : AbstractValidator<CancelServiceTicketCommand>
{
    public CancelServiceTicketCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
