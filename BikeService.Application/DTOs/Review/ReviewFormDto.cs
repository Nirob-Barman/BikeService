namespace BikeService.Application.DTOs.Review
{
    public class ReviewFormDto
    {
        public int ServiceTicketId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
    }
}
