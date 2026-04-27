using BikeService.Domain.Enums;

namespace BikeService.Application.DTOs.LeaveRequest
{
    public class LeaveRequestDto
    {
        public int Id { get; set; }
        public int MechanicId { get; set; }
        public string MechanicName { get; set; } = string.Empty;
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int TotalDays => (ToDate - FromDate).Days + 1;
        public LeaveType Type { get; set; }
        public string? Reason { get; set; }
        public LeaveRequestStatus Status { get; set; }
        public string? AdminNotes { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
