using BikeService.Domain.Enums;

namespace BikeService.Application.DTOs.LeaveRequest
{
    public class LeaveRequestFormDto
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public LeaveType Type { get; set; }
        public string? Reason { get; set; }
    }
}
