using FluentValidation;

namespace BikeService.Application.Features.Appointments.Commands.ConvertAppointmentToTicket;

public class ConvertAppointmentToTicketCommandValidator : AbstractValidator<ConvertAppointmentToTicketCommand>
{
    public ConvertAppointmentToTicketCommandValidator()
    {
        RuleFor(x => x.AppointmentId).GreaterThan(0);
    }
}
