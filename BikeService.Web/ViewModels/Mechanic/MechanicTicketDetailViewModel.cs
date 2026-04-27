using BikeService.Application.DTOs.Part;
using BikeService.Application.DTOs.ServiceTicket;
using BikeService.Application.DTOs.ServiceType;
using BikeService.Application.DTOs.TicketNote;
using BikeService.Domain.Enums;

namespace BikeService.Web.ViewModels.Mechanic
{
    public class MechanicTicketDetailViewModel
    {
        public ServiceTicketDto Ticket { get; set; } = new();
        public List<ServiceTypeDto> ServiceTypes { get; set; } = new();
        public List<PartDto> Parts { get; set; } = new();
        public List<TicketNoteDto> Notes { get; set; } = new();

        public ServiceTicketStatus? NextStatus => Ticket.Status switch
        {
            ServiceTicketStatus.Pending      => ServiceTicketStatus.Diagnosed,
            ServiceTicketStatus.Diagnosed    => ServiceTicketStatus.InProgress,
            ServiceTicketStatus.InProgress   => ServiceTicketStatus.QualityCheck,
            ServiceTicketStatus.QualityCheck => ServiceTicketStatus.ReadyForPickup,
            _ => null
        };
    }
}
