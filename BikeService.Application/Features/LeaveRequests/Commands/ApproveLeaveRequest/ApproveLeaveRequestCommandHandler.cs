using System.Text.Json;
using BikeService.Application.Interfaces;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;
using BikeService.Domain.Enums;
using MediatR;

namespace BikeService.Application.Features.LeaveRequests.Commands.ApproveLeaveRequest;

public class ApproveLeaveRequestCommandHandler(
    IUnitOfWork unitOfWork,
    IAuditLogService auditLogService,
    IUserContextService userContextService) : IRequestHandler<ApproveLeaveRequestCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(ApproveLeaveRequestCommand request, CancellationToken cancellationToken)
    {
        var entity = await unitOfWork.Repository<LeaveRequest>().GetByIdAsync(request.Id);
        if (entity == null)
            return Result<bool>.Fail("Leave request not found.");

        if (entity.Status != LeaveRequestStatus.Pending)
            return Result<bool>.Fail("Only pending requests can be approved.");

        var oldStatus = entity.Status.ToString();
        entity.Status     = LeaveRequestStatus.Approved;
        entity.AdminNotes = request.AdminNotes?.Trim();
        entity.UpdatedBy  = userContextService.UserId;
        entity.UpdatedAt  = DateTime.UtcNow;

        unitOfWork.Repository<LeaveRequest>().Update(entity);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await auditLogService.LogAsync("LeaveRequest", "Approve",
            userContextService.UserId, userContextService.Email,
            $"Leave request #{request.Id} approved",
            entityId: request.Id.ToString(),
            ipAddress: userContextService.IpAddress,
            userAgent: userContextService.UserAgent,
            oldValues: JsonSerializer.Serialize(new { Status = oldStatus }),
            newValues: JsonSerializer.Serialize(new { Status = "Approved", AdminNotes = request.AdminNotes }));

        return Result<bool>.Ok(true, "Leave request approved.");
    }
}
