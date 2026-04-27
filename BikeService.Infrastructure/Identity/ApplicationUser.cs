using Microsoft.AspNetCore.Identity;

namespace BikeService.Infrastructure.Identity
{
    public class ApplicationUser : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? ProfileImageUrl { get; set; }
        public string? Address { get; set; }
        public bool IsBanned { get; set; }      // set LockoutEnd = DateTimeOffset.MaxValue to ban
    }
}
