namespace BikeService.Application.DTOs.AuditLog
{
    public class AuditLogPagedResultDto
    {
        public List<AuditLogDto> Items { get; set; } = new();
        public int TotalCount { get; set; }
    }
}
