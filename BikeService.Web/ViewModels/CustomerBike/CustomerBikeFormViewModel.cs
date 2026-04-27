using System.ComponentModel.DataAnnotations;

namespace BikeService.Web.ViewModels.CustomerBike
{
    public class CustomerBikeFormViewModel
    {
        [Required]
        [StringLength(100)]
        [Display(Name = "Make")]
        public string Make { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Display(Name = "Model")]
        public string Model { get; set; } = string.Empty;

        [Required]
        [Range(1900, 2100, ErrorMessage = "Year must be between 1900 and 2100.")]
        [Display(Name = "Year")]
        public int Year { get; set; } = DateTime.Now.Year;

        [StringLength(50)]
        [Display(Name = "Registration No.")]
        public string? RegistrationNo { get; set; }

        public string? ExistingImageUrl { get; set; }

        [Display(Name = "Bike Photo")]
        public IFormFile? Image { get; set; }
    }
}
