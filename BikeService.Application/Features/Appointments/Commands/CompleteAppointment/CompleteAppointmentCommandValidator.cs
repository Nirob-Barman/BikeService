using FluentValidation;

namespace BikeService.Application.Features.Appointments.Commands.CompleteAppointment;

public class CompleteAppointmentCommandValidator : AbstractValidator<CompleteAppointmentCommand>
{
    public CompleteAppointmentCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
