using FluentValidation;

namespace BikeService.Application.Features.ServiceTickets.Commands.CreateServiceTicket;

public class CreateServiceTicketCommandValidator : AbstractValidator<CreateServiceTicketCommand>
{
    public CreateServiceTicketCommandValidator()
    {
        RuleFor(x => x.BikeId).GreaterThan(0);
    }
}
