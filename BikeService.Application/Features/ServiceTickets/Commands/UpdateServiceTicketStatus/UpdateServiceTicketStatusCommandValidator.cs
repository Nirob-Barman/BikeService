using FluentValidation;

namespace BikeService.Application.Features.ServiceTickets.Commands.UpdateServiceTicketStatus;

public class UpdateServiceTicketStatusCommandValidator : AbstractValidator<UpdateServiceTicketStatusCommand>
{
    public UpdateServiceTicketStatusCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
