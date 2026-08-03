using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.Appointments.Commands.ConfirmAppointment;

public record ConfirmAppointmentCommand(int Id) : IRequest<Result<bool>>;
