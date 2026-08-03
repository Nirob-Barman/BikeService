using BikeService.Application.DTOs.Appointment;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Mappers;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;
using MediatR;

namespace BikeService.Application.Features.Appointments.Queries.GetAppointmentById;

public class GetAppointmentByIdQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetAppointmentByIdQuery, Result<AppointmentDto>>
{
    public async Task<Result<AppointmentDto>> Handle(GetAppointmentByIdQuery request, CancellationToken cancellationToken)
    {
        var appointments = await unitOfWork.Repository<Appointment>()
            .GetAllWithIncludesAsync<Appointment>(
                a => a.Id == request.Id,
                a => a,
                a => a.Bike,
                a => a.ServiceTickets);

        var appointment = appointments.FirstOrDefault();
        if (appointment is null)
            return Result<AppointmentDto>.Fail("Appointment not found.");

        return Result<AppointmentDto>.Ok(AppointmentMapper.ToDto(appointment, appointment.Bike));
    }
}
