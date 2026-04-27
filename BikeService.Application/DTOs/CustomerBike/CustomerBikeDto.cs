namespace BikeService.Application.DTOs.CustomerBike
{
    public class CustomerBikeDto
    {
        public int Id { get; set; }
        public string Make { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int Year { get; set; }
        public string? RegistrationNo { get; set; }
        public string? ImageUrl { get; set; }
        public string CustomerId { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
