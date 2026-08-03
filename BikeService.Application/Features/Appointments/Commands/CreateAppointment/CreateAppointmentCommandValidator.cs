using FluentValidation;

namespace BikeService.Application.Features.Appointments.Commands.CreateAppointment;

public class CreateAppointmentCommandValidator : AbstractValidator<CreateAppointmentCommand>
{
    public CreateAppointmentCommandValidator()
    {
        RuleFor(x => x.BikeId).GreaterThan(0);
        RuleFor(x => x.AppointmentDate).NotEmpty();
    }
}
