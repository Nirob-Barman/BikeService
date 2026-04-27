using BikeService.Application.DTOs.CustomerBike;
using System.ComponentModel.DataAnnotations;

namespace BikeService.Web.ViewModels.Appointment
{
    public class AppointmentCreateViewModel
    {
        [Required(ErrorMessage = "Please select a bike.")]
        [Display(Name = "Bike")]
        public int BikeId { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        [Display(Name = "Appointment Date & Time")]
        public DateTime AppointmentDate { get; set; } = DateTime.Today.AddDays(1);

        [StringLength(500)]
        [Display(Name = "Notes (optional)")]
        public string? Notes { get; set; }

        public List<CustomerBikeDto> Bikes { get; set; } = new();
    }
}
