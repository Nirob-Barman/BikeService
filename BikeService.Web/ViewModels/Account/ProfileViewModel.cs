using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace BikeService.Web.ViewModels.Account;

public class ProfileViewModel
{
    [Required(ErrorMessage = "First name is required.")]
    [StringLength(50)]
    [Display(Name = "First Name")]
    public string? FirstName { get; set; }

    [Required(ErrorMessage = "Last name is required.")]
    [StringLength(50)]
    [Display(Name = "Last Name")]
    public string? LastName { get; set; }

    [StringLength(200)]
    public string? Address { get; set; }

    // Display only — not submitted on form
    public string? Email { get; set; }
    public string? ProfileImageUrl { get; set; }

    [Display(Name = "Profile Photo")]
    public IFormFile? Photo { get; set; }
}
