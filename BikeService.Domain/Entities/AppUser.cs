
namespace BikeService.Domain.Entities
{
    public class AppUser
    {
        public string? Id { get; set; }
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Address { get; set; }
        public bool IsBanned { get; set; }
        public string? ProfileImageUrl { get; set; }

        public string FullName => $"{FirstName} {LastName}".Trim();
    }
}
