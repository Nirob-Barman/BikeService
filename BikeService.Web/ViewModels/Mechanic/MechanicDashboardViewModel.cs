using BikeService.Application.DTOs.LeaveRequest;
using BikeService.Application.DTOs.ServiceTicket;
using BikeService.Domain.Enums;

namespace BikeService.Web.ViewModels.Mechanic
{
    public class MechanicDashboardViewModel
    {
        public List<ServiceTicketDto> ActiveTickets { get; set; } = new();
        public List<LeaveRequestDto> RecentLeave { get; set; } = new();

        // Computed stats
        public int TotalActive    => ActiveTickets.Count;
        public int OverdueCount   => ActiveTickets.Count(t => t.IsOverdue);
        public int InProgressCount => ActiveTickets.Count(t => t.Status == ServiceTicketStatus.InProgress);
        public int ReadyCount      => ActiveTickets.Count(t => t.Status == ServiceTicketStatus.ReadyForPickup);
        public int PendingLeaveCount => RecentLeave.Count(l => l.Status == LeaveRequestStatus.Pending);
    }
}
