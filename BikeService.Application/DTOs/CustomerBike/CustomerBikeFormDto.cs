using System.ComponentModel.DataAnnotations;

namespace BikeService.Application.DTOs.CustomerBike
{
    public class CustomerBikeFormDto
    {
        [Required]
        public string Make { get; set; } = string.Empty;

        [Required]
        public string Model { get; set; } = string.Empty;

        [Range(1900, 2100, ErrorMessage = "Year must be between 1900 and 2100.")]
        public int Year { get; set; }

        public string? RegistrationNo { get; set; }

        public string? ImageUrl { get; set; }
    }
}
