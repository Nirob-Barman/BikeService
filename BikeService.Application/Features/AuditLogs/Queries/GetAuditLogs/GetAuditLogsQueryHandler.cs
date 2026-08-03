using BikeService.Application.DTOs.AuditLog;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;
using MediatR;

namespace BikeService.Application.Features.AuditLogs.Queries.GetAuditLogs;

public class GetAuditLogsQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetAuditLogsQuery, Result<AuditLogPagedResultDto>>
{
    public async Task<Result<AuditLogPagedResultDto>> Handle(GetAuditLogsQuery request, CancellationToken cancellationToken)
    {
        var filter = request.Filter;
        var query = unitOfWork.Repository<AuditLog>().GetAllAsQueryable();

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

        var totalCount = await unitOfWork.Repository<AuditLog>().CountAsync(query);

        query = query.OrderByDescending(l => l.CreatedAt);

        var pageNumber = filter.PageNumber < 1 ? 1 : filter.PageNumber;
        var pageSize = filter.PageSize < 1 ? 20 : filter.PageSize;

        query = unitOfWork.Repository<AuditLog>().PaginateAsQueryable(query, pageNumber, pageSize);

        var items = await unitOfWork.Repository<AuditLog>().ToListAsync(query);

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

        return Result<AuditLogPagedResultDto>.Ok(new AuditLogPagedResultDto { Items = dtos, TotalCount = totalCount });
    }
}
