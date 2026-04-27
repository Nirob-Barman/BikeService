namespace BikeService.Application.DTOs.Review
{
    public class ReviewDto
    {
        public int Id { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public int ServiceTicketId { get; set; }
        public string BikeSummary { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
