using System.Text.Json;
using BikeService.Application.Interfaces;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;
using BikeService.Domain.Enums;
using MediatR;

namespace BikeService.Application.Features.Appointments.Commands.CompleteAppointment;

public class CompleteAppointmentCommandHandler(
    IUnitOfWork unitOfWork,
    IAuditLogService auditLogService,
    IUserContextService userContextService) : IRequestHandler<CompleteAppointmentCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(CompleteAppointmentCommand request, CancellationToken cancellationToken)
    {
        var appointment = await unitOfWork.Repository<Appointment>().GetByIdAsync(request.Id);
        if (appointment is null)
            return Result<bool>.Fail("Appointment not found.");

        if (appointment.Status != AppointmentStatus.Confirmed)
            return Result<bool>.Fail($"Cannot complete an appointment with status '{appointment.Status}'. Only Confirmed appointments can be marked as completed.");

        var oldValues = JsonSerializer.Serialize(new { appointment.Status });

        appointment.Status = AppointmentStatus.Completed;
        appointment.UpdatedAt = DateTime.UtcNow;
        appointment.UpdatedBy = userContextService.UserId;

        unitOfWork.Repository<Appointment>().Update(appointment);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await auditLogService.LogAsync(
            "Appointment", "Complete",
            userContextService.UserId, userContextService.Email,
            $"Appointment #{request.Id} marked as completed.",
            entityId: request.Id.ToString(),
            ipAddress: userContextService.IpAddress,
            userAgent: userContextService.UserAgent,
            oldValues: oldValues,
            newValues: JsonSerializer.Serialize(new { Status = AppointmentStatus.Completed }));

        return Result<bool>.Ok(true, "Appointment marked as completed.");
    }
}
