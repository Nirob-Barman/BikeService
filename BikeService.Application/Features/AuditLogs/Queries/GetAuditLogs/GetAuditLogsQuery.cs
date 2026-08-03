using BikeService.Application.DTOs.AuditLog;
using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.AuditLogs.Queries.GetAuditLogs;

public record GetAuditLogsQuery(AuditLogFilterDto Filter) : IRequest<Result<AuditLogPagedResultDto>>;
