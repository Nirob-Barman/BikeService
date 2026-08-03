using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.Appointments.Commands.CompleteAppointment;

public record CompleteAppointmentCommand(int Id) : IRequest<Result<bool>>;
