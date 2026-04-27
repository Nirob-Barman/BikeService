using BikeService.Application.DTOs.Appointment;
using BikeService.Domain.Entities;
using BikeService.Domain.Enums;

namespace BikeService.Application.Mappers
{
    public static class AppointmentMapper
    {
        public static AppointmentDto ToDto(Appointment appointment, CustomerBike? bike = null, string customerName = "") => new()
        {
            Id = appointment.Id,
            AppointmentDate = appointment.AppointmentDate,
            Notes = appointment.Notes,
            Status = appointment.Status,
            BikeId = appointment.BikeId,
            BikeSummary = bike is not null
                ? $"{bike.Year} {bike.Make} {bike.Model}"
                : string.Empty,
            CustomerId = appointment.CustomerId,
            CustomerName = customerName,
            CreatedAt = appointment.CreatedAt,
            HasTicket = appointment.ServiceTickets?.Any() ?? false
        };

        public static Appointment ToEntity(AppointmentFormDto dto) => new()
        {
            AppointmentDate = dto.AppointmentDate,
            Notes = dto.Notes,
            BikeId = dto.BikeId,
            Status = AppointmentStatus.Scheduled
        };

        public static void UpdateEntity(Appointment appointment, AppointmentFormDto dto)
        {
            appointment.AppointmentDate = dto.AppointmentDate;
            appointment.Notes = dto.Notes;
            appointment.BikeId = dto.BikeId;
        }
    }
}
