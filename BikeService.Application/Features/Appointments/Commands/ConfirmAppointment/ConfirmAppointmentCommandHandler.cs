using System.Text.Json;
using BikeService.Application.Interfaces;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;
using BikeService.Domain.Enums;
using MediatR;

namespace BikeService.Application.Features.Appointments.Commands.ConfirmAppointment;

public class ConfirmAppointmentCommandHandler(
    IUnitOfWork unitOfWork,
    IAuditLogService auditLogService,
    IUserContextService userContextService) : IRequestHandler<ConfirmAppointmentCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(ConfirmAppointmentCommand request, CancellationToken cancellationToken)
    {
        var appointment = await unitOfWork.Repository<Appointment>().GetByIdAsync(request.Id);
        if (appointment is null)
            return Result<bool>.Fail("Appointment not found.");

        if (appointment.Status != AppointmentStatus.Scheduled)
            return Result<bool>.Fail($"Cannot confirm an appointment with status '{appointment.Status}'. Only Scheduled appointments can be confirmed.");

        var oldValues = JsonSerializer.Serialize(new { appointment.Status });

        appointment.Status = AppointmentStatus.Confirmed;
        appointment.UpdatedAt = DateTime.UtcNow;
        appointment.UpdatedBy = userContextService.UserId;

        unitOfWork.Repository<Appointment>().Update(appointment);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await auditLogService.LogAsync(
            "Appointment", "Confirm",
            userContextService.UserId, userContextService.Email,
            $"Appointment #{request.Id} confirmed.",
            entityId: request.Id.ToString(),
            ipAddress: userContextService.IpAddress,
            userAgent: userContextService.UserAgent,
            oldValues: oldValues,
            newValues: JsonSerializer.Serialize(new { Status = AppointmentStatus.Confirmed }));

        return Result<bool>.Ok(true, "Appointment confirmed.");
    }
}
