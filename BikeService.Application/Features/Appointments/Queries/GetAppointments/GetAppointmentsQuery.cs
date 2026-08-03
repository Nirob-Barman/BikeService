using BikeService.Application.DTOs.Appointment;
using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.Appointments.Queries.GetAppointments;

public record GetAppointmentsQuery(AppointmentFilterDto? Filter = null) : IRequest<Result<List<AppointmentDto>>>;
