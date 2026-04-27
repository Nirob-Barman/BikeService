using BikeService.Application.DTOs.Payroll;
using BikeService.Application.Interfaces;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Interfaces.Services;
using BikeService.Application.Mappers;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;
using BikeService.Domain.Enums;

namespace BikeService.Application.Services
{
    public class PayrollService : IPayrollService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditLogService _auditLogService;
        private readonly IUserContextService _userContextService;

        public PayrollService(
            IUnitOfWork unitOfWork,
            IAuditLogService auditLogService,
            IUserContextService userContextService)
        {
            _unitOfWork = unitOfWork;
            _auditLogService = auditLogService;
            _userContextService = userContextService;
        }

        public async Task<Result<List<PayrollRecordDto>>> GetAllAsync(int? year = null)
        {
            var records = await _unitOfWork.Repository<PayrollRecord>()
                .GetAllWithIncludesAsync(
                    predicate: p => year == null || p.Year == year,
                    selector: p => PayrollRecordMapper.ToDto(p),
                    includes: p => p.Mechanic);

            return Result<List<PayrollRecordDto>>.Ok(
                records.OrderByDescending(r => r.Year).ThenByDescending(r => r.Month).ToList());
        }

        public async Task<Result<List<PayrollRecordDto>>> GetMyPayrollAsync()
        {
            var mechanic = await _unitOfWork.Repository<Mechanic>()
                .FirstOrDefaultAsync(m => m.UserId == _userContextService.UserId);

            if (mechanic == null)
                return Result<List<PayrollRecordDto>>.Fail("Mechanic profile not found.");

            return await GetByMechanicAsync(mechanic.Id);
        }

        public async Task<Result<List<PayrollRecordDto>>> GetByMechanicAsync(int mechanicId)
        {
            var records = await _unitOfWork.Repository<PayrollRecord>()
                .GetAllWithIncludesAsync(
                    predicate: p => p.MechanicId == mechanicId,
                    selector: p => PayrollRecordMapper.ToDto(p),
                    includes: p => p.Mechanic);

            return Result<List<PayrollRecordDto>>.Ok(
                records.OrderByDescending(r => r.Year).ThenByDescending(r => r.Month).ToList());
        }

        public async Task<Result<PayrollRecordDto>> GetByIdAsync(int id)
        {
            var records = await _unitOfWork.Repository<PayrollRecord>()
                .GetAllWithIncludesAsync(
                    predicate: p => p.Id == id,
                    selector: p => PayrollRecordMapper.ToDto(p),
                    includes: p => p.Mechanic);

            var dto = records.FirstOrDefault();
            if (dto == null)
                return Result<PayrollRecordDto>.Fail("Payroll record not found.");

            return Result<PayrollRecordDto>.Ok(dto);
        }

        public async Task<Result<int>> CreateAsync(PayrollRecordFormDto dto)
        {
            var mechanic = await _unitOfWork.Repository<Mechanic>().GetByIdAsync(dto.MechanicId);
            if (mechanic == null)
                return Result<int>.Fail("Mechanic not found.");

            var exists = await _unitOfWork.Repository<PayrollRecord>()
                .AnyAsync(p => p.MechanicId == dto.MechanicId && p.Month == dto.Month && p.Year == dto.Year);
            if (exists)
                return Result<int>.Fail($"A payroll record for {mechanic.FullName} in {new DateTime(dto.Year, dto.Month, 1):MMMM yyyy} already exists.");

            if (dto.BaseSalary < 0 || dto.Bonus < 0 || dto.Deductions < 0)
                return Result<int>.Fail("Salary amounts cannot be negative.");

            var entity = new PayrollRecord
            {
                MechanicId = dto.MechanicId,
                Month      = dto.Month,
                Year       = dto.Year,
                BaseSalary = dto.BaseSalary,
                Bonus      = dto.Bonus,
                Deductions = dto.Deductions,
                Notes      = dto.Notes?.Trim(),
                Status     = PayrollStatus.Draft,
                CreatedAt  = DateTime.UtcNow,
                CreatedBy  = _userContextService.UserId,
            };

            await _unitOfWork.Repository<PayrollRecord>().AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            await _auditLogService.LogAsync("PayrollRecord", "Create",
                _userContextService.UserId, _userContextService.Email,
                $"Payroll record created for mechanic #{dto.MechanicId} ({new DateTime(dto.Year, dto.Month, 1):MMMM yyyy})",
                entityId: entity.Id.ToString(),
                ipAddress: _userContextService.IpAddress,
                userAgent: _userContextService.UserAgent,
                newValues: System.Text.Json.JsonSerializer.Serialize(new
                {
                    entity.MechanicId, entity.Month, entity.Year,
                    entity.BaseSalary, entity.Bonus, entity.Deductions
                }));

            return Result<int>.Ok(entity.Id, "Payroll record created.");
        }

        public async Task<Result<bool>> UpdateAsync(int id, PayrollRecordFormDto dto)
        {
            var entity = await _unitOfWork.Repository<PayrollRecord>().GetByIdAsync(id);
            if (entity == null)
                return Result<bool>.Fail("Payroll record not found.");

            if (entity.Status != PayrollStatus.Draft)
                return Result<bool>.Fail("Only Draft records can be edited.");

            if (dto.BaseSalary < 0 || dto.Bonus < 0 || dto.Deductions < 0)
                return Result<bool>.Fail("Salary amounts cannot be negative.");

            var oldValues = System.Text.Json.JsonSerializer.Serialize(new
            {
                entity.BaseSalary, entity.Bonus, entity.Deductions, entity.Notes
            });

            entity.BaseSalary = dto.BaseSalary;
            entity.Bonus      = dto.Bonus;
            entity.Deductions = dto.Deductions;
            entity.Notes      = dto.Notes?.Trim();
            entity.UpdatedAt  = DateTime.UtcNow;
            entity.UpdatedBy  = _userContextService.UserId;

            _unitOfWork.Repository<PayrollRecord>().Update(entity);
            await _unitOfWork.SaveChangesAsync();

            await _auditLogService.LogAsync("PayrollRecord", "Update",
                _userContextService.UserId, _userContextService.Email,
                $"Payroll record #{id} updated",
                entityId: id.ToString(),
                ipAddress: _userContextService.IpAddress,
                userAgent: _userContextService.UserAgent,
                oldValues: oldValues,
                newValues: System.Text.Json.JsonSerializer.Serialize(new
                {
                    entity.BaseSalary, entity.Bonus, entity.Deductions, entity.Notes
                }));

            return Result<bool>.Ok(true, "Payroll record updated.");
        }

        public async Task<Result<bool>> FinalizeAsync(int id)
        {
            var entity = await _unitOfWork.Repository<PayrollRecord>().GetByIdAsync(id);
            if (entity == null)
                return Result<bool>.Fail("Payroll record not found.");

            if (entity.Status != PayrollStatus.Draft)
                return Result<bool>.Fail("Only Draft records can be finalized.");

            entity.Status    = PayrollStatus.Finalized;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = _userContextService.UserId;

            _unitOfWork.Repository<PayrollRecord>().Update(entity);
            await _unitOfWork.SaveChangesAsync();

            await _auditLogService.LogAsync("PayrollRecord", "Finalize",
                _userContextService.UserId, _userContextService.Email,
                $"Payroll record #{id} finalized",
                entityId: id.ToString(),
                ipAddress: _userContextService.IpAddress,
                userAgent: _userContextService.UserAgent,
                oldValues: System.Text.Json.JsonSerializer.Serialize(new { Status = "Draft" }),
                newValues: System.Text.Json.JsonSerializer.Serialize(new { Status = "Finalized" }));

            return Result<bool>.Ok(true, "Payroll record finalized.");
        }

        public async Task<Result<bool>> MarkPaidAsync(int id)
        {
            var entity = await _unitOfWork.Repository<PayrollRecord>().GetByIdAsync(id);
            if (entity == null)
                return Result<bool>.Fail("Payroll record not found.");

            if (entity.Status != PayrollStatus.Finalized)
                return Result<bool>.Fail("Only Finalized records can be marked as paid.");

            entity.Status    = PayrollStatus.Paid;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = _userContextService.UserId;

            _unitOfWork.Repository<PayrollRecord>().Update(entity);
            await _unitOfWork.SaveChangesAsync();

            await _auditLogService.LogAsync("PayrollRecord", "MarkPaid",
                _userContextService.UserId, _userContextService.Email,
                $"Payroll record #{id} marked as paid",
                entityId: id.ToString(),
                ipAddress: _userContextService.IpAddress,
                userAgent: _userContextService.UserAgent,
                oldValues: System.Text.Json.JsonSerializer.Serialize(new { Status = "Finalized" }),
                newValues: System.Text.Json.JsonSerializer.Serialize(new { Status = "Paid" }));

            return Result<bool>.Ok(true, "Payroll marked as paid.");
        }

        public async Task<Result<bool>> DeleteAsync(int id)
        {
            var entity = await _unitOfWork.Repository<PayrollRecord>().GetByIdAsync(id);
            if (entity == null)
                return Result<bool>.Fail("Payroll record not found.");

            if (entity.Status != PayrollStatus.Draft)
                return Result<bool>.Fail("Only Draft records can be deleted.");

            var oldValues = System.Text.Json.JsonSerializer.Serialize(new
            {
                entity.MechanicId, entity.Month, entity.Year,
                entity.BaseSalary, entity.Bonus, entity.Deductions
            });

            _unitOfWork.Repository<PayrollRecord>().Remove(entity);
            await _unitOfWork.SaveChangesAsync();

            await _auditLogService.LogAsync("PayrollRecord", "Delete",
                _userContextService.UserId, _userContextService.Email,
                $"Payroll record #{id} deleted",
                entityId: id.ToString(),
                ipAddress: _userContextService.IpAddress,
                userAgent: _userContextService.UserAgent,
                oldValues: oldValues);

            return Result<bool>.Ok(true, "Payroll record deleted.");
        }
    }
}
