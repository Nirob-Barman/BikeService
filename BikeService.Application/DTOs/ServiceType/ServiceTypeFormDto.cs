using System.ComponentModel.DataAnnotations;

namespace BikeService.Application.DTOs.ServiceType
{
    public class ServiceTypeFormDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public decimal BasePrice { get; set; }

        public double EstimatedHours { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
