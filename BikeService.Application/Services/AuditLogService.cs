using BikeService.Application.DTOs.AuditLog;
using BikeService.Application.Interfaces;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;

namespace BikeService.Application.Services
{
    public class AuditLogService : IAuditLogService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AuditLogService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task LogAsync(
            string entityName,
            string action,
            string? userId,
            string? userEmail,
            string details,
            string? entityId = null,
            string? ipAddress = null,
            string? userAgent = null,
            string? oldValues = null,
            string? newValues = null)
        {
            var log = new AuditLog
            {
                EntityName = entityName,
                Action = action,
                UserId = userId,
                UserEmail = userEmail,
                Details = details,
                EntityId = entityId,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                OldValues = oldValues,
                NewValues = newValues,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<AuditLog>().AddAsync(log);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<Result<(List<AuditLogDto> Items, int TotalCount)>> GetPagedAsync(AuditLogFilterDto filter)
        {
            var query = _unitOfWork.Repository<AuditLog>().GetAllAsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.EntityName))
                query = query.Where(l => l.EntityName == filter.EntityName);

            if (!string.IsNullOrWhiteSpace(filter.Action))
                query = query.Where(l => l.Action == filter.Action);

            if (!string.IsNullOrWhiteSpace(filter.UserEmail))
                query = query.Where(l => l.UserEmail != null && l.UserEmail.Contains(filter.UserEmail));

            if (filter.DateFrom.HasValue)
                query = query.Where(l => l.CreatedAt >= filter.DateFrom.Value);

            if (filter.DateTo.HasValue)
                query = query.Where(l => l.CreatedAt <= filter.DateTo.Value);

            var totalCount = await _unitOfWork.Repository<AuditLog>().CountAsync(query);

            query = query.OrderByDescending(l => l.CreatedAt);

            var pageNumber = filter.PageNumber < 1 ? 1 : filter.PageNumber;
            var pageSize = filter.PageSize < 1 ? 20 : filter.PageSize;

            query = _unitOfWork.Repository<AuditLog>().PaginateAsQueryable(query, pageNumber, pageSize);

            var items = await _unitOfWork.Repository<AuditLog>().ToListAsync(query);

            var dtos = items.Select(l => new AuditLogDto
            {
                Id = l.Id,
                EntityName = l.EntityName,
                Action = l.Action,
                EntityId = l.EntityId,
                UserId = l.UserId,
                UserEmail = l.UserEmail,
                Details = l.Details,
                OldValues = l.OldValues,
                NewValues = l.NewValues,
                IpAddress = l.IpAddress,
                UserAgent = l.UserAgent,
                CreatedAt = l.CreatedAt
            }).ToList();

            return Result<(List<AuditLogDto> Items, int TotalCount)>.Ok((dtos, totalCount));
        }
    }
}
