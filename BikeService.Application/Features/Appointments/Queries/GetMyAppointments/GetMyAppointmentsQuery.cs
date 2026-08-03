using BikeService.Application.DTOs.Appointment;
using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.Appointments.Queries.GetMyAppointments;

public record GetMyAppointmentsQuery : IRequest<Result<List<AppointmentDto>>>;
