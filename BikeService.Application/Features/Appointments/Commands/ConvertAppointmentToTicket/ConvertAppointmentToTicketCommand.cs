using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.Appointments.Commands.ConvertAppointmentToTicket;

public record ConvertAppointmentToTicketCommand(int AppointmentId) : IRequest<Result<int>>;
