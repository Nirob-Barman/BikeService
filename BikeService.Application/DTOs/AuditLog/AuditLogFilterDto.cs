namespace BikeService.Application.DTOs.AuditLog
{
    public class AuditLogFilterDto
    {
        public string? EntityName { get; set; }
        public string? Action { get; set; }
        public string? UserEmail { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
