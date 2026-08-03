using System.Text.Json;
using BikeService.Application.Interfaces;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;
using BikeService.Domain.Enums;
using MediatR;

namespace BikeService.Application.Features.LeaveRequests.Commands.CancelLeaveRequest;

public class CancelLeaveRequestCommandHandler(
    IUnitOfWork unitOfWork,
    IAuditLogService auditLogService,
    IUserContextService userContextService) : IRequestHandler<CancelLeaveRequestCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(CancelLeaveRequestCommand request, CancellationToken cancellationToken)
    {
        var entity = await unitOfWork.Repository<LeaveRequest>().GetByIdAsync(request.Id);
        if (entity == null)
            return Result<bool>.Fail("Leave request not found.");

        if (entity.Status != LeaveRequestStatus.Pending)
            return Result<bool>.Fail("Only pending requests can be cancelled.");

        entity.Status    = LeaveRequestStatus.Cancelled;
        entity.UpdatedBy = userContextService.UserId;
        entity.UpdatedAt = DateTime.UtcNow;

        unitOfWork.Repository<LeaveRequest>().Update(entity);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await auditLogService.LogAsync("LeaveRequest", "Cancel",
            userContextService.UserId, userContextService.Email,
            $"Leave request #{request.Id} cancelled",
            entityId: request.Id.ToString(),
            ipAddress: userContextService.IpAddress,
            userAgent: userContextService.UserAgent,
            oldValues: JsonSerializer.Serialize(new { Status = "Pending" }),
            newValues: JsonSerializer.Serialize(new { Status = "Cancelled" }));

        return Result<bool>.Ok(true, "Leave request cancelled.");
    }
}
