using System.Text.Json;
using BikeService.Application.Interfaces;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;
using BikeService.Domain.Enums;
using MediatR;

namespace BikeService.Application.Features.Payroll.Commands.DeletePayrollRecord;

public class DeletePayrollRecordCommandHandler(
    IUnitOfWork unitOfWork,
    IAuditLogService auditLogService,
    IUserContextService userContextService) : IRequestHandler<DeletePayrollRecordCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(DeletePayrollRecordCommand request, CancellationToken cancellationToken)
    {
        var entity = await unitOfWork.Repository<PayrollRecord>().GetByIdAsync(request.Id);
        if (entity == null)
            return Result<bool>.Fail("Payroll record not found.");

        if (entity.Status != PayrollStatus.Draft)
            return Result<bool>.Fail("Only Draft records can be deleted.");

        var oldValues = JsonSerializer.Serialize(new
        {
            entity.MechanicId, entity.Month, entity.Year,
            entity.BaseSalary, entity.Bonus, entity.Deductions
        });

        unitOfWork.Repository<PayrollRecord>().Remove(entity);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await auditLogService.LogAsync("PayrollRecord", "Delete",
            userContextService.UserId, userContextService.Email,
            $"Payroll record #{request.Id} deleted",
            entityId: request.Id.ToString(),
            ipAddress: userContextService.IpAddress,
            userAgent: userContextService.UserAgent,
            oldValues: oldValues);

        return Result<bool>.Ok(true, "Payroll record deleted.");
    }
}
