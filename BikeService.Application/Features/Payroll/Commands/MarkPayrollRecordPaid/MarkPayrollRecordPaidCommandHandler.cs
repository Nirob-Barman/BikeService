using System.Text.Json;
using BikeService.Application.Interfaces;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;
using BikeService.Domain.Enums;
using MediatR;

namespace BikeService.Application.Features.Payroll.Commands.MarkPayrollRecordPaid;

public class MarkPayrollRecordPaidCommandHandler(
    IUnitOfWork unitOfWork,
    IAuditLogService auditLogService,
    IUserContextService userContextService) : IRequestHandler<MarkPayrollRecordPaidCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(MarkPayrollRecordPaidCommand request, CancellationToken cancellationToken)
    {
        var entity = await unitOfWork.Repository<PayrollRecord>().GetByIdAsync(request.Id);
        if (entity == null)
            return Result<bool>.Fail("Payroll record not found.");

        if (entity.Status != PayrollStatus.Finalized)
            return Result<bool>.Fail("Only Finalized records can be marked as paid.");

        entity.Status    = PayrollStatus.Paid;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = userContextService.UserId;

        unitOfWork.Repository<PayrollRecord>().Update(entity);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await auditLogService.LogAsync("PayrollRecord", "MarkPaid",
            userContextService.UserId, userContextService.Email,
            $"Payroll record #{request.Id} marked as paid",
            entityId: request.Id.ToString(),
            ipAddress: userContextService.IpAddress,
            userAgent: userContextService.UserAgent,
            oldValues: JsonSerializer.Serialize(new { Status = "Finalized" }),
            newValues: JsonSerializer.Serialize(new { Status = "Paid" }));

        return Result<bool>.Ok(true, "Payroll marked as paid.");
    }
}
