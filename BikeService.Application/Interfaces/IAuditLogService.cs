using BikeService.Application.DTOs.AuditLog;
using BikeService.Application.Wrappers;

namespace BikeService.Application.Interfaces
{
    public interface IAuditLogService
    {
        Task LogAsync(
            string entityName,
            string action,
            string? userId,
            string? userEmail,
            string details,
            string? entityId = null,
            string? ipAddress = null,
            string? userAgent = null,
            string? oldValues = null,
            string? newValues = null);

        Task<Result<(List<AuditLogDto> Items, int TotalCount)>> GetPagedAsync(AuditLogFilterDto filter);
    }
}
