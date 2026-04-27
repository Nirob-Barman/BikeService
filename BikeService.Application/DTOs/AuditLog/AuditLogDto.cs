namespace BikeService.Application.DTOs.AuditLog
{
    public class AuditLogDto
    {
        public int Id { get; set; }
        public string EntityName { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string? EntityId { get; set; }
        public string? UserId { get; set; }
        public string? UserEmail { get; set; }
        public string? Details { get; set; }
        public string? OldValues { get; set; }
        public string? NewValues { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
