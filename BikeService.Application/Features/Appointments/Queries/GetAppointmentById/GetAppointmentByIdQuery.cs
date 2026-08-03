using BikeService.Application.DTOs.Appointment;
using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.Appointments.Queries.GetAppointmentById;

public record GetAppointmentByIdQuery(int Id) : IRequest<Result<AppointmentDto>>;
