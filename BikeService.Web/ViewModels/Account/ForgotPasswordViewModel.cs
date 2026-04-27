using System.ComponentModel.DataAnnotations;

namespace BikeService.Web.ViewModels.Account
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        [MinLength(5, ErrorMessage = "Email must be at least 5 characters.")]
        public string? Email { get; set; }
    }
}
