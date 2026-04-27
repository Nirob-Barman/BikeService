using System.Text.Json;
using BikeService.Application.DTOs.Appointment;
using BikeService.Application.Interfaces;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Interfaces.Services;
using BikeService.Application.Mappers;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;
using BikeService.Domain.Enums;

namespace BikeService.Application.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditLogService _auditLogService;
        private readonly IUserContextService _userContextService;

        public AppointmentService(
            IUnitOfWork unitOfWork,
            IAuditLogService auditLogService,
            IUserContextService userContextService)
        {
            _unitOfWork = unitOfWork;
            _auditLogService = auditLogService;
            _userContextService = userContextService;
        }

        public async Task<Result<List<AppointmentDto>>> GetAllAsync(AppointmentFilterDto? filter = null)
        {
            var appointments = await _unitOfWork.Repository<Appointment>()
                .GetAllWithIncludesAsync<Appointment>(
                    a => true,
                    a => a,
                    a => a.Bike);

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

        public async Task<Result<AppointmentDto>> GetByIdAsync(int id)
        {
            var appointments = await _unitOfWork.Repository<Appointment>()
                .GetAllWithIncludesAsync<Appointment>(
                    a => a.Id == id,
                    a => a,
                    a => a.Bike,
                    a => a.ServiceTickets);

            var appointment = appointments.FirstOrDefault();
            if (appointment is null)
                return Result<AppointmentDto>.Fail("Appointment not found.");

            return Result<AppointmentDto>.Ok(AppointmentMapper.ToDto(appointment, appointment.Bike));
        }

        public async Task<Result<List<AppointmentDto>>> GetMyAppointmentsAsync()
        {
            var userId = _userContextService.UserId;
            if (string.IsNullOrEmpty(userId))
                return Result<List<AppointmentDto>>.Fail("User is not authenticated.");

            var appointments = await _unitOfWork.Repository<Appointment>()
                .GetAllWithIncludesAsync<Appointment>(
                    a => a.CustomerId == userId,
                    a => a,
                    a => a.Bike);

            var dtos = appointments
                .Select(a => AppointmentMapper.ToDto(a, a.Bike))
                .ToList();

            return Result<List<AppointmentDto>>.Ok(dtos);
        }

        public async Task<Result<int>> CreateAsync(AppointmentFormDto dto)
        {
            var userId = _userContextService.UserId;
            if (string.IsNullOrEmpty(userId))
                return Result<int>.Fail("User is not authenticated.");

            // Verify bike belongs to current user
            var bike = await _unitOfWork.Repository<CustomerBike>().GetByIdAsync(dto.BikeId);
            if (bike is null)
                return Result<int>.Fail("Bike not found.");

            if (bike.CustomerId != userId)
                return Result<int>.Fail("You do not have permission to book an appointment for this bike.");

            if (dto.AppointmentDate <= DateTime.UtcNow)
                return Result<int>.FailField("AppointmentDate", "Appointment date must be in the future.");

            var appointment = AppointmentMapper.ToEntity(dto);
            appointment.CustomerId = userId;
            appointment.CreatedBy = userId;

            await _unitOfWork.Repository<Appointment>().AddAsync(appointment);
            await _unitOfWork.SaveChangesAsync();

            await _auditLogService.LogAsync(
                "Appointment", "Create",
                _userContextService.UserId, _userContextService.Email,
                $"Appointment created for bike '{bike.Make} {bike.Model}' on {appointment.AppointmentDate:yyyy-MM-dd}",
                entityId: appointment.Id.ToString(),
                ipAddress: _userContextService.IpAddress,
                userAgent: _userContextService.UserAgent,
                newValues: JsonSerializer.Serialize(new { appointment.AppointmentDate, appointment.BikeId, appointment.Status, appointment.Notes }));

            return Result<int>.Ok(appointment.Id, "Appointment booked successfully.");
        }

        public async Task<Result<bool>> ConfirmAsync(int id)
        {
            var appointment = await _unitOfWork.Repository<Appointment>().GetByIdAsync(id);
            if (appointment is null)
                return Result<bool>.Fail("Appointment not found.");

            if (appointment.Status != AppointmentStatus.Scheduled)
                return Result<bool>.Fail($"Cannot confirm an appointment with status '{appointment.Status}'. Only Scheduled appointments can be confirmed.");

            var oldValues = JsonSerializer.Serialize(new { appointment.Status });

            appointment.Status = AppointmentStatus.Confirmed;
            appointment.UpdatedAt = DateTime.UtcNow;
            appointment.UpdatedBy = _userContextService.UserId;

            _unitOfWork.Repository<Appointment>().Update(appointment);
            await _unitOfWork.SaveChangesAsync();

            await _auditLogService.LogAsync(
                "Appointment", "Confirm",
                _userContextService.UserId, _userContextService.Email,
                $"Appointment #{id} confirmed.",
                entityId: id.ToString(),
                ipAddress: _userContextService.IpAddress,
                userAgent: _userContextService.UserAgent,
                oldValues: oldValues,
                newValues: JsonSerializer.Serialize(new { Status = AppointmentStatus.Confirmed }));

            return Result<bool>.Ok(true, "Appointment confirmed.");
        }

        public async Task<Result<bool>> CancelAsync(int id)
        {
            var appointment = await _unitOfWork.Repository<Appointment>().GetByIdAsync(id);
            if (appointment is null)
                return Result<bool>.Fail("Appointment not found.");

            if (appointment.Status == AppointmentStatus.Cancelled)
                return Result<bool>.Fail("Appointment is already cancelled.");

            if (appointment.Status == AppointmentStatus.Completed)
                return Result<bool>.Fail("Cannot cancel a completed appointment.");

            var oldValues = JsonSerializer.Serialize(new { appointment.Status });

            appointment.Status = AppointmentStatus.Cancelled;
            appointment.UpdatedAt = DateTime.UtcNow;
            appointment.UpdatedBy = _userContextService.UserId;

            _unitOfWork.Repository<Appointment>().Update(appointment);
            await _unitOfWork.SaveChangesAsync();

            await _auditLogService.LogAsync(
                "Appointment", "Cancel",
                _userContextService.UserId, _userContextService.Email,
                $"Appointment #{id} cancelled.",
                entityId: id.ToString(),
                ipAddress: _userContextService.IpAddress,
                userAgent: _userContextService.UserAgent,
                oldValues: oldValues,
                newValues: JsonSerializer.Serialize(new { Status = AppointmentStatus.Cancelled }));

            return Result<bool>.Ok(true, "Appointment cancelled.");
        }

        public async Task<Result<bool>> CompleteAsync(int id)
        {
            var appointment = await _unitOfWork.Repository<Appointment>().GetByIdAsync(id);
            if (appointment is null)
                return Result<bool>.Fail("Appointment not found.");

            if (appointment.Status != AppointmentStatus.Confirmed)
                return Result<bool>.Fail($"Cannot complete an appointment with status '{appointment.Status}'. Only Confirmed appointments can be marked as completed.");

            var oldValues = JsonSerializer.Serialize(new { appointment.Status });

            appointment.Status = AppointmentStatus.Completed;
            appointment.UpdatedAt = DateTime.UtcNow;
            appointment.UpdatedBy = _userContextService.UserId;

            _unitOfWork.Repository<Appointment>().Update(appointment);
            await _unitOfWork.SaveChangesAsync();

            await _auditLogService.LogAsync(
                "Appointment", "Complete",
                _userContextService.UserId, _userContextService.Email,
                $"Appointment #{id} marked as completed.",
                entityId: id.ToString(),
                ipAddress: _userContextService.IpAddress,
                userAgent: _userContextService.UserAgent,
                oldValues: oldValues,
                newValues: JsonSerializer.Serialize(new { Status = AppointmentStatus.Completed }));

            return Result<bool>.Ok(true, "Appointment marked as completed.");
        }
    }
}
