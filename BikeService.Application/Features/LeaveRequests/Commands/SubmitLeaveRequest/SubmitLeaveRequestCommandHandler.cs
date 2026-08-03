using System.Text.Json;
using BikeService.Application.Interfaces;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;
using BikeService.Domain.Enums;
using MediatR;

namespace BikeService.Application.Features.LeaveRequests.Commands.SubmitLeaveRequest;

public class SubmitLeaveRequestCommandHandler(
    IUnitOfWork unitOfWork,
    IAuditLogService auditLogService,
    IUserContextService userContextService) : IRequestHandler<SubmitLeaveRequestCommand, Result<int>>
{
    public async Task<Result<int>> Handle(SubmitLeaveRequestCommand request, CancellationToken cancellationToken)
    {
        var mechanic = await unitOfWork.Repository<Mechanic>()
            .FirstOrDefaultAsync(m => m.UserId == userContextService.UserId);

        if (mechanic == null)
            return Result<int>.Fail("Mechanic profile not found.");

        var mechanicId = mechanic.Id;

        if (request.FromDate.Date < DateTime.UtcNow.Date)
            return Result<int>.FailField("FromDate", "Start date cannot be in the past.");

        if (request.ToDate < request.FromDate)
            return Result<int>.FailField("ToDate", "End date must be on or after the start date.");

        var hasOverlap = await unitOfWork.Repository<LeaveRequest>().AnyAsync(l =>
            l.MechanicId == mechanicId &&
            (l.Status == LeaveRequestStatus.Pending || l.Status == LeaveRequestStatus.Approved) &&
            l.FromDate <= request.ToDate && l.ToDate >= request.FromDate);

        if (hasOverlap)
            return Result<int>.Fail("A pending or approved leave request already exists for the selected dates.");

        var entity = new LeaveRequest
        {
            MechanicId = mechanicId,
            FromDate   = request.FromDate.Date,
            ToDate     = request.ToDate.Date,
            Type       = request.Type,
            Reason     = request.Reason?.Trim(),
            Status     = LeaveRequestStatus.Pending,
            CreatedBy  = userContextService.UserId,
            CreatedAt  = DateTime.UtcNow,
        };

        await unitOfWork.Repository<LeaveRequest>().AddAsync(entity);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await auditLogService.LogAsync("LeaveRequest", "Create",
            userContextService.UserId, userContextService.Email,
            $"Leave request submitted ({request.Type}, {request.FromDate:d} – {request.ToDate:d})",
            entityId: entity.Id.ToString(),
            ipAddress: userContextService.IpAddress,
            userAgent: userContextService.UserAgent,
            newValues: JsonSerializer.Serialize(new
            {
                entity.MechanicId, entity.FromDate, entity.ToDate,
                Type = entity.Type.ToString(), entity.Reason
            }));

        return Result<int>.Ok(entity.Id, "Leave request submitted.");
    }
}
