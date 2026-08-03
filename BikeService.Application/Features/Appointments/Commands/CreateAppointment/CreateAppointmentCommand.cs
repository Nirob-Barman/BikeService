using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.Appointments.Commands.CreateAppointment;

public record CreateAppointmentCommand(
    int BikeId,
    DateTime AppointmentDate,
    string? Notes) : IRequest<Result<int>>;
