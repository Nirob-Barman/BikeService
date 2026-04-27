namespace BikeService.Application.DTOs.Customer
{
    public class CustomerDto
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public bool IsBanned { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
