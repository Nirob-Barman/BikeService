using System.Text.Json;
using BikeService.Application.Interfaces;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;
using BikeService.Domain.Enums;
using MediatR;

namespace BikeService.Application.Features.Appointments.Commands.CancelAppointment;

public class CancelAppointmentCommandHandler(
    IUnitOfWork unitOfWork,
    IAuditLogService auditLogService,
    IUserContextService userContextService) : IRequestHandler<CancelAppointmentCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(CancelAppointmentCommand request, CancellationToken cancellationToken)
    {
        var appointment = await unitOfWork.Repository<Appointment>().GetByIdAsync(request.Id);
        if (appointment is null)
            return Result<bool>.Fail("Appointment not found.");

        if (appointment.Status == AppointmentStatus.Cancelled)
            return Result<bool>.Fail("Appointment is already cancelled.");

        if (appointment.Status == AppointmentStatus.Completed)
            return Result<bool>.Fail("Cannot cancel a completed appointment.");

        var oldValues = JsonSerializer.Serialize(new { appointment.Status });

        appointment.Status = AppointmentStatus.Cancelled;
        appointment.UpdatedAt = DateTime.UtcNow;
        appointment.UpdatedBy = userContextService.UserId;

        unitOfWork.Repository<Appointment>().Update(appointment);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await auditLogService.LogAsync(
            "Appointment", "Cancel",
            userContextService.UserId, userContextService.Email,
            $"Appointment #{request.Id} cancelled.",
            entityId: request.Id.ToString(),
            ipAddress: userContextService.IpAddress,
            userAgent: userContextService.UserAgent,
            oldValues: oldValues,
            newValues: JsonSerializer.Serialize(new { Status = AppointmentStatus.Cancelled }));

        return Result<bool>.Ok(true, "Appointment cancelled.");
    }
}
