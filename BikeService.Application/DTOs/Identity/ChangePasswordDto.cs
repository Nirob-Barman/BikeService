
using System.ComponentModel.DataAnnotations;

namespace BikeService.Application.DTOs.Identity
{
    public class ChangePasswordDto
    {
        public string? CurrentPassword { get; set; }
        public string? NewPassword { get; set; }
        public string? ConfirmPassword { get; set; }
    }
}
