using System.Text.Json;
using BikeService.Application.Interfaces;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;
using BikeService.Domain.Enums;
using MediatR;

namespace BikeService.Application.Features.Payroll.Commands.CreatePayrollRecord;

public class CreatePayrollRecordCommandHandler(
    IUnitOfWork unitOfWork,
    IAuditLogService auditLogService,
    IUserContextService userContextService) : IRequestHandler<CreatePayrollRecordCommand, Result<int>>
{
    public async Task<Result<int>> Handle(CreatePayrollRecordCommand request, CancellationToken cancellationToken)
    {
        var mechanic = await unitOfWork.Repository<Mechanic>().GetByIdAsync(request.MechanicId);
        if (mechanic == null)
            return Result<int>.Fail("Mechanic not found.");

        var exists = await unitOfWork.Repository<PayrollRecord>()
            .AnyAsync(p => p.MechanicId == request.MechanicId && p.Month == request.Month && p.Year == request.Year);
        if (exists)
            return Result<int>.Fail($"A payroll record for {mechanic.FullName} in {new DateTime(request.Year, request.Month, 1):MMMM yyyy} already exists.");

        if (request.BaseSalary < 0 || request.Bonus < 0 || request.Deductions < 0)
            return Result<int>.Fail("Salary amounts cannot be negative.");

        var entity = new PayrollRecord
        {
            MechanicId = request.MechanicId,
            Month      = request.Month,
            Year       = request.Year,
            BaseSalary = request.BaseSalary,
            Bonus      = request.Bonus,
            Deductions = request.Deductions,
            Notes      = request.Notes?.Trim(),
            Status     = PayrollStatus.Draft,
            CreatedAt  = DateTime.UtcNow,
            CreatedBy  = userContextService.UserId,
        };

        await unitOfWork.Repository<PayrollRecord>().AddAsync(entity);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await auditLogService.LogAsync("PayrollRecord", "Create",
            userContextService.UserId, userContextService.Email,
            $"Payroll record created for mechanic #{request.MechanicId} ({new DateTime(request.Year, request.Month, 1):MMMM yyyy})",
            entityId: entity.Id.ToString(),
            ipAddress: userContextService.IpAddress,
            userAgent: userContextService.UserAgent,
            newValues: JsonSerializer.Serialize(new
            {
                entity.MechanicId, entity.Month, entity.Year,
                entity.BaseSalary, entity.Bonus, entity.Deductions
            }));

        return Result<int>.Ok(entity.Id, "Payroll record created.");
    }
}
