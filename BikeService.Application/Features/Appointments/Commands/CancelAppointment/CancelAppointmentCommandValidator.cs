using FluentValidation;

namespace BikeService.Application.Features.Appointments.Commands.CancelAppointment;

public class CancelAppointmentCommandValidator : AbstractValidator<CancelAppointmentCommand>
{
    public CancelAppointmentCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
