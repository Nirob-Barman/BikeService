using BikeService.Application.DTOs.Appointment;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Mappers;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;
using MediatR;

namespace BikeService.Application.Features.Appointments.Queries.GetAppointments;

public class GetAppointmentsQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetAppointmentsQuery, Result<List<AppointmentDto>>>
{
    public async Task<Result<List<AppointmentDto>>> Handle(GetAppointmentsQuery request, CancellationToken cancellationToken)
    {
        var appointments = await unitOfWork.Repository<Appointment>()
            .GetAllWithIncludesAsync<Appointment>(
                a => true,
                a => a,
                a => a.Bike);

        var filter = request.Filter;
        if (filter is not null)
        {
            if (filter.Status.HasValue)
                appointments = appointments.Where(a => a.Status == filter.Status.Value);

            if (!string.IsNullOrEmpty(filter.CustomerId))
                appointments = appointments.Where(a => a.CustomerId == filter.CustomerId);

            if (filter.DateFrom.HasValue)
                appointments = appointments.Where(a => a.AppointmentDate >= filter.DateFrom.Value);

            if (filter.DateTo.HasValue)
                appointments = appointments.Where(a => a.AppointmentDate <= filter.DateTo.Value);
        }

        var dtos = appointments
            .Select(a => AppointmentMapper.ToDto(a, a.Bike))
            .ToList();

        return Result<List<AppointmentDto>>.Ok(dtos);
    }
}
