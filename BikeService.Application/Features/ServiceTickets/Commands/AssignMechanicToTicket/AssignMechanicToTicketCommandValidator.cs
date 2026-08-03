using FluentValidation;

namespace BikeService.Application.Features.ServiceTickets.Commands.AssignMechanicToTicket;

public class AssignMechanicToTicketCommandValidator : AbstractValidator<AssignMechanicToTicketCommand>
{
    public AssignMechanicToTicketCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.MechanicId).GreaterThan(0);
    }
}
