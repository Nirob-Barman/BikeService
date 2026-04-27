namespace BikeService.Domain.Entities;

public class Mechanic : BaseEntity
{
    public string FullName { get; set; } = string.Empty;
    public string? Specialty { get; set; }
    public bool IsAvailable { get; set; } = true;

    public string? UserId { get; set; }

    public ICollection<ServiceTicket> ServiceTickets { get; set; } = [];
    public ICollection<LeaveRequest> LeaveRequests { get; set; } = [];
}
