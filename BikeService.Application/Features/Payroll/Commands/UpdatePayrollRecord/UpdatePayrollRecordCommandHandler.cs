using System.Text.Json;
using BikeService.Application.Interfaces;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;
using BikeService.Domain.Enums;
using MediatR;

namespace BikeService.Application.Features.Payroll.Commands.UpdatePayrollRecord;

public class UpdatePayrollRecordCommandHandler(
    IUnitOfWork unitOfWork,
    IAuditLogService auditLogService,
    IUserContextService userContextService) : IRequestHandler<UpdatePayrollRecordCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(UpdatePayrollRecordCommand request, CancellationToken cancellationToken)
    {
        var entity = await unitOfWork.Repository<PayrollRecord>().GetByIdAsync(request.Id);
        if (entity == null)
            return Result<bool>.Fail("Payroll record not found.");

        if (entity.Status != PayrollStatus.Draft)
            return Result<bool>.Fail("Only Draft records can be edited.");

        if (request.BaseSalary < 0 || request.Bonus < 0 || request.Deductions < 0)
            return Result<bool>.Fail("Salary amounts cannot be negative.");

        var oldValues = JsonSerializer.Serialize(new
        {
            entity.BaseSalary, entity.Bonus, entity.Deductions, entity.Notes
        });

        entity.BaseSalary = request.BaseSalary;
        entity.Bonus      = request.Bonus;
        entity.Deductions = request.Deductions;
        entity.Notes      = request.Notes?.Trim();
        entity.UpdatedAt  = DateTime.UtcNow;
        entity.UpdatedBy  = userContextService.UserId;

        unitOfWork.Repository<PayrollRecord>().Update(entity);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await auditLogService.LogAsync("PayrollRecord", "Update",
            userContextService.UserId, userContextService.Email,
            $"Payroll record #{request.Id} updated",
            entityId: request.Id.ToString(),
            ipAddress: userContextService.IpAddress,
            userAgent: userContextService.UserAgent,
            oldValues: oldValues,
            newValues: JsonSerializer.Serialize(new
            {
                entity.BaseSalary, entity.Bonus, entity.Deductions, entity.Notes
            }));

        return Result<bool>.Ok(true, "Payroll record updated.");
    }
}
