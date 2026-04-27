using System.ComponentModel.DataAnnotations;

namespace BikeService.Web.ViewModels.ServiceType
{
    public class ServiceTypeFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        [MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
        [Display(Name = "Service Name")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Base price is required.")]
        [Range(0.01, 999999.99, ErrorMessage = "Base price must be greater than 0.")]
        [Display(Name = "Base Price ($)")]
        public decimal BasePrice { get; set; }

        [Required(ErrorMessage = "Estimated hours is required.")]
        [Range(0.1, 999.0, ErrorMessage = "Estimated hours must be greater than 0.")]
        [Display(Name = "Estimated Hours")]
        public double EstimatedHours { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
    }
}
