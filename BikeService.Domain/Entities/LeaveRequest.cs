using BikeService.Domain.Enums;

namespace BikeService.Domain.Entities
{
    public class LeaveRequest : BaseEntity
    {
        public int MechanicId { get; set; }
        public Mechanic Mechanic { get; set; } = null!;

        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public LeaveType Type { get; set; }
        public string? Reason { get; set; }

        public LeaveRequestStatus Status { get; set; } = LeaveRequestStatus.Pending;
        public string? AdminNotes { get; set; }
    }
}
