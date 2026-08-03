using System.Text.Json;
using BikeService.Application.Interfaces;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;
using BikeService.Domain.Enums;
using MediatR;

namespace BikeService.Application.Features.Payroll.Commands.FinalizePayrollRecord;

public class FinalizePayrollRecordCommandHandler(
    IUnitOfWork unitOfWork,
    IAuditLogService auditLogService,
    IUserContextService userContextService) : IRequestHandler<FinalizePayrollRecordCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(FinalizePayrollRecordCommand request, CancellationToken cancellationToken)
    {
        var entity = await unitOfWork.Repository<PayrollRecord>().GetByIdAsync(request.Id);
        if (entity == null)
            return Result<bool>.Fail("Payroll record not found.");

        if (entity.Status != PayrollStatus.Draft)
            return Result<bool>.Fail("Only Draft records can be finalized.");

        entity.Status    = PayrollStatus.Finalized;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = userContextService.UserId;

        unitOfWork.Repository<PayrollRecord>().Update(entity);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await auditLogService.LogAsync("PayrollRecord", "Finalize",
            userContextService.UserId, userContextService.Email,
            $"Payroll record #{request.Id} finalized",
            entityId: request.Id.ToString(),
            ipAddress: userContextService.IpAddress,
            userAgent: userContextService.UserAgent,
            oldValues: JsonSerializer.Serialize(new { Status = "Draft" }),
            newValues: JsonSerializer.Serialize(new { Status = "Finalized" }));

        return Result<bool>.Ok(true, "Payroll record finalized.");
    }
}
