using BikeService.Application.DTOs.Mechanic;
using BikeService.Application.DTOs.PromoCode;
using BikeService.Application.DTOs.Review;
using BikeService.Application.DTOs.ServiceType;

namespace BikeService.Web.ViewModels.Home
{
    public class HomeViewModel
    {
        public List<ServiceTypeDto> ServiceTypes { get; set; } = new();
        public List<MechanicDto> Mechanics { get; set; } = new();
        public List<PromoCodeDto> PromoCodes { get; set; } = new();
        public List<ReviewDto> Reviews { get; set; } = new();
        public int TotalBikesServiced { get; set; }
        public int TotalCustomers { get; set; }
        public int CompletedTickets { get; set; }
    }
}
