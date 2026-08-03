using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.Appointments.Commands.CancelAppointment;

public record CancelAppointmentCommand(int Id) : IRequest<Result<bool>>;
