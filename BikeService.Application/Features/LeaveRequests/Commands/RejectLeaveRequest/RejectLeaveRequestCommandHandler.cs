using System.Text.Json;
using BikeService.Application.Interfaces;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;
using BikeService.Domain.Enums;
using MediatR;

namespace BikeService.Application.Features.LeaveRequests.Commands.RejectLeaveRequest;

public class RejectLeaveRequestCommandHandler(
    IUnitOfWork unitOfWork,
    IAuditLogService auditLogService,
    IUserContextService userContextService) : IRequestHandler<RejectLeaveRequestCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(RejectLeaveRequestCommand request, CancellationToken cancellationToken)
    {
        var entity = await unitOfWork.Repository<LeaveRequest>().GetByIdAsync(request.Id);
        if (entity == null)
            return Result<bool>.Fail("Leave request not found.");

        if (entity.Status != LeaveRequestStatus.Pending)
            return Result<bool>.Fail("Only pending requests can be rejected.");

        var oldStatus = entity.Status.ToString();
        entity.Status     = LeaveRequestStatus.Rejected;
        entity.AdminNotes = request.AdminNotes?.Trim();
        entity.UpdatedBy  = userContextService.UserId;
        entity.UpdatedAt  = DateTime.UtcNow;

        unitOfWork.Repository<LeaveRequest>().Update(entity);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await auditLogService.LogAsync("LeaveRequest", "Reject",
            userContextService.UserId, userContextService.Email,
            $"Leave request #{request.Id} rejected",
            entityId: request.Id.ToString(),
            ipAddress: userContextService.IpAddress,
            userAgent: userContextService.UserAgent,
            oldValues: JsonSerializer.Serialize(new { Status = oldStatus }),
            newValues: JsonSerializer.Serialize(new { Status = "Rejected", AdminNotes = request.AdminNotes }));

        return Result<bool>.Ok(true, "Leave request rejected.");
    }
}
