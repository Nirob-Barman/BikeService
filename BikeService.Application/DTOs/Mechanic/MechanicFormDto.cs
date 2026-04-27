using System.ComponentModel.DataAnnotations;

namespace BikeService.Application.DTOs.Mechanic
{
    public class MechanicFormDto
    {
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        public string? Specialty { get; set; }

        public bool IsAvailable { get; set; } = true;

        // Login account — populated on Create only
        public string? Email { get; set; }
        public string? Password { get; set; }
    }
}
