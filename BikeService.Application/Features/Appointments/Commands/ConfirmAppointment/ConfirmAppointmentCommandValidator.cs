using FluentValidation;

namespace BikeService.Application.Features.Appointments.Commands.ConfirmAppointment;

public class ConfirmAppointmentCommandValidator : AbstractValidator<ConfirmAppointmentCommand>
{
    public ConfirmAppointmentCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
