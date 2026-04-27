using System.ComponentModel.DataAnnotations;

namespace BikeService.Web.ViewModels.Mechanic
{
    public class MechanicFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Full name is required.")]
        [MaxLength(100, ErrorMessage = "Full name cannot exceed 100 characters.")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [MaxLength(100, ErrorMessage = "Specialty cannot exceed 100 characters.")]
        [Display(Name = "Specialty")]
        public string? Specialty { get; set; }

        [Display(Name = "Available")]
        public bool IsAvailable { get; set; } = true;

        // ── Login account (Create only) ───────────────────────────────────────
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        [Display(Name = "Login Email")]
        public string? Email { get; set; }

        [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
        [Display(Name = "Temporary Password")]
        public string? Password { get; set; }

        [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
        [Display(Name = "Confirm Password")]
        public string? ConfirmPassword { get; set; }

        // ── Populated on Edit GET — shows currently linked account ────────────
        public string? LinkedEmail { get; set; }
        public bool IsLoginActive { get; set; }
    }
}
