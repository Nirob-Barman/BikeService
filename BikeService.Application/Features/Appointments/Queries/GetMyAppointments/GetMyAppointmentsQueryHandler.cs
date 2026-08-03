using BikeService.Application.DTOs.Appointment;
using BikeService.Application.Interfaces;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Mappers;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;
using MediatR;

namespace BikeService.Application.Features.Appointments.Queries.GetMyAppointments;

public class GetMyAppointmentsQueryHandler(
    IUnitOfWork unitOfWork,
    IUserContextService userContextService) : IRequestHandler<GetMyAppointmentsQuery, Result<List<AppointmentDto>>>
{
    public async Task<Result<List<AppointmentDto>>> Handle(GetMyAppointmentsQuery request, CancellationToken cancellationToken)
    {
        var userId = userContextService.UserId;
        if (string.IsNullOrEmpty(userId))
            return Result<List<AppointmentDto>>.Fail("User is not authenticated.");

        var appointments = await unitOfWork.Repository<Appointment>()
            .GetAllWithIncludesAsync<Appointment>(
                a => a.CustomerId == userId,
                a => a,
                a => a.Bike);

        var dtos = appointments
            .Select(a => AppointmentMapper.ToDto(a, a.Bike))
            .ToList();

        return Result<List<AppointmentDto>>.Ok(dtos);
    }
}
