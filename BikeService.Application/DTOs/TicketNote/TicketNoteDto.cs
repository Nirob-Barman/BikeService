namespace BikeService.Application.DTOs.TicketNote
{
    public class TicketNoteDto
    {
        public int Id { get; set; }
        public int ServiceTicketId { get; set; }
        public string AuthorId { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public string AuthorRole { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
