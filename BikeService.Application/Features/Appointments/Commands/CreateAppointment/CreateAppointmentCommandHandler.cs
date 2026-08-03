using System.Text.Json;
using BikeService.Application.Interfaces;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Mappers;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;
using MediatR;

namespace BikeService.Application.Features.Appointments.Commands.CreateAppointment;

public class CreateAppointmentCommandHandler(
    IUnitOfWork unitOfWork,
    IAuditLogService auditLogService,
    IUserContextService userContextService) : IRequestHandler<CreateAppointmentCommand, Result<int>>
{
    public async Task<Result<int>> Handle(CreateAppointmentCommand request, CancellationToken cancellationToken)
    {
        var userId = userContextService.UserId;
        if (string.IsNullOrEmpty(userId))
            return Result<int>.Fail("User is not authenticated.");

        var bike = await unitOfWork.Repository<CustomerBike>().GetByIdAsync(request.BikeId);
        if (bike is null)
            return Result<int>.Fail("Bike not found.");

        if (bike.CustomerId != userId)
            return Result<int>.Fail("You do not have permission to book an appointment for this bike.");

        if (request.AppointmentDate <= DateTime.UtcNow)
            return Result<int>.FailField("AppointmentDate", "Appointment date must be in the future.");

        var appointment = AppointmentMapper.ToEntity(new()
        {
            BikeId = request.BikeId,
            AppointmentDate = request.AppointmentDate,
            Notes = request.Notes
        });
        appointment.CustomerId = userId;
        appointment.CreatedBy = userId;

        await unitOfWork.Repository<Appointment>().AddAsync(appointment);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await auditLogService.LogAsync(
            "Appointment", "Create",
            userContextService.UserId, userContextService.Email,
            $"Appointment created for bike '{bike.Make} {bike.Model}' on {appointment.AppointmentDate:yyyy-MM-dd}",
            entityId: appointment.Id.ToString(),
            ipAddress: userContextService.IpAddress,
            userAgent: userContextService.UserAgent,
            newValues: JsonSerializer.Serialize(new { appointment.AppointmentDate, appointment.BikeId, appointment.Status, appointment.Notes }));

        return Result<int>.Ok(appointment.Id, "Appointment booked successfully.");
    }
}
