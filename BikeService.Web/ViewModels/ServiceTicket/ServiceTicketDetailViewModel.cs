using BikeService.Application.DTOs.ServiceTicket;
using BikeService.Application.DTOs.TicketNote;

namespace BikeService.Web.ViewModels.ServiceTicket
{
    public class ServiceTicketDetailViewModel
    {
        public ServiceTicketDto Ticket { get; set; } = new();
        public List<TicketNoteDto> Notes { get; set; } = new();
    }
}
