using BikeService.Application.DTOs.LeaveRequest;
using BikeService.Domain.Entities;

namespace BikeService.Application.Mappers
{
    public static class LeaveRequestMapper
    {
        public static LeaveRequestDto ToDto(LeaveRequest e) => new()
        {
            Id           = e.Id,
            MechanicId   = e.MechanicId,
            MechanicName = e.Mechanic?.FullName ?? string.Empty,
            FromDate     = e.FromDate,
            ToDate       = e.ToDate,
            Type         = e.Type,
            Reason       = e.Reason,
            Status       = e.Status,
            AdminNotes   = e.AdminNotes,
            CreatedAt    = e.CreatedAt,
        };
    }
}
