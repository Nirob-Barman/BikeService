using BikeService.Application.DTOs.Mechanic;
using BikeService.Application.DTOs.Part;
using BikeService.Application.DTOs.ServiceTicket;
using BikeService.Application.DTOs.ServiceType;

namespace BikeService.Web.ViewModels.ServiceTicket
{
    public class TicketDetailViewModel
    {
        public ServiceTicketDto Ticket { get; set; } = null!;
        public List<MechanicDto> AvailableMechanics { get; set; } = new();
        public List<ServiceTypeDto> ActiveServiceTypes { get; set; } = new();
        public List<PartDto> AllParts { get; set; } = new();
    }
}
