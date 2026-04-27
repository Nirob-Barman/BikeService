using System.Text.Json;
using BikeService.Application.DTOs.PromoCode;
using BikeService.Application.Interfaces;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Interfaces.Services;
using BikeService.Application.Mappers;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;

namespace BikeService.Application.Services
{
    public class PromoCodeService : IPromoCodeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditLogService _auditLogService;
        private readonly IUserContextService _userContextService;

        public PromoCodeService(
            IUnitOfWork unitOfWork,
            IAuditLogService auditLogService,
            IUserContextService userContextService)
        {
            _unitOfWork = unitOfWork;
            _auditLogService = auditLogService;
            _userContextService = userContextService;
        }

        public async Task<Result<List<PromoCodeDto>>> GetAllAsync()
        {
            var items = await _unitOfWork.Repository<PromoCode>()
                .GetAllAsync<PromoCodeDto>(e => PromoCodeMapper.ToDto(e));
            return Result<List<PromoCodeDto>>.Ok(items.ToList());
        }

        public async Task<Result<PromoCodeDto>> GetByIdAsync(int id)
        {
            var entity = await _unitOfWork.Repository<PromoCode>().GetByIdAsync(id);
            if (entity == null)
                return Result<PromoCodeDto>.Fail("Promo code not found.");
            return Result<PromoCodeDto>.Ok(PromoCodeMapper.ToDto(entity));
        }

        public async Task<Result<List<PromoCodeDto>>> GetActiveAsync()
        {
            var items = await _unitOfWork.Repository<PromoCode>()
                .GetAllAsync<PromoCodeDto>(e => e.IsActive, e => PromoCodeMapper.ToDto(e));
            return Result<List<PromoCodeDto>>.Ok(items.ToList());
        }

        public async Task<Result<PromoCodeDto>> ValidateCodeAsync(string code)
        {
            var entity = await _unitOfWork.Repository<PromoCode>()
                .FirstOrDefaultAsync(e => e.Code == code);

            if (entity == null)
                return Result<PromoCodeDto>.Fail("Promo code not found.");

            if (!entity.IsActive)
                return Result<PromoCodeDto>.Fail("This promo code is no longer active.");

            if (entity.ExpiresAt.HasValue && entity.ExpiresAt.Value < DateTime.UtcNow)
                return Result<PromoCodeDto>.Fail("This promo code has expired.");

            if (entity.UsageCount >= entity.MaxUsages)
                return Result<PromoCodeDto>.Fail("This promo code has reached its maximum usage limit.");

            return Result<PromoCodeDto>.Ok(PromoCodeMapper.ToDto(entity), "Promo code is valid.");
        }

        public async Task<Result<int>> CreateAsync(PromoCodeFormDto dto)
        {
            var duplicate = await _unitOfWork.Repository<PromoCode>()
                .AnyAsync(e => e.Code == dto.Code);
            if (duplicate)
                return Result<int>.FailField("Code", "A promo code with this code already exists.");

            var entity = PromoCodeMapper.ToEntity(dto);
            entity.CreatedBy = _userContextService.UserId;
            entity.CreatedAt = DateTime.UtcNow;

            await _unitOfWork.Repository<PromoCode>().AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            await _auditLogService.LogAsync(
                "PromoCode", "Create",
                _userContextService.UserId, _userContextService.Email,
                $"Created promo code '{entity.Code}'",
                entityId: entity.Id.ToString(),
                ipAddress: _userContextService.IpAddress,
                userAgent: _userContextService.UserAgent,
                oldValues: null,
                newValues: JsonSerializer.Serialize(new
                {
                    entity.Code,
                    entity.DiscountPercent,
                    entity.MaxUsages,
                    entity.ExpiresAt,
                    entity.IsActive
                }));

            return Result<int>.Ok(entity.Id, "Promo code created successfully.");
        }

        public async Task<Result<bool>> UpdateAsync(int id, PromoCodeFormDto dto)
        {
            var entity = await _unitOfWork.Repository<PromoCode>().GetByIdAsync(id);
            if (entity == null)
                return Result<bool>.Fail("Promo code not found.");

            var duplicate = await _unitOfWork.Repository<PromoCode>()
                .AnyAsync(e => e.Code == dto.Code && e.Id != id);
            if (duplicate)
                return Result<bool>.FailField("Code", "A promo code with this code already exists.");

            var oldValues = JsonSerializer.Serialize(new
            {
                entity.Code,
                entity.DiscountPercent,
                entity.MaxUsages,
                entity.ExpiresAt,
                entity.IsActive
            });

            PromoCodeMapper.UpdateEntity(entity, dto);
            entity.UpdatedBy = _userContextService.UserId;
            entity.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Repository<PromoCode>().Update(entity);
            await _unitOfWork.SaveChangesAsync();

            await _auditLogService.LogAsync(
                "PromoCode", "Update",
                _userContextService.UserId, _userContextService.Email,
                $"Updated promo code '{entity.Code}'",
                entityId: entity.Id.ToString(),
                ipAddress: _userContextService.IpAddress,
                userAgent: _userContextService.UserAgent,
                oldValues: oldValues,
                newValues: JsonSerializer.Serialize(new
                {
                    entity.Code,
                    entity.DiscountPercent,
                    entity.MaxUsages,
                    entity.ExpiresAt,
                    entity.IsActive
                }));

            return Result<bool>.Ok(true, "Promo code updated successfully.");
        }

        public async Task<Result<bool>> ToggleActiveAsync(int id)
        {
            var entity = await _unitOfWork.Repository<PromoCode>().GetByIdAsync(id);
            if (entity == null)
                return Result<bool>.Fail("Promo code not found.");

            var oldValues = JsonSerializer.Serialize(new { entity.IsActive });

            entity.IsActive = !entity.IsActive;
            entity.UpdatedBy = _userContextService.UserId;
            entity.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Repository<PromoCode>().Update(entity);
            await _unitOfWork.SaveChangesAsync();

            await _auditLogService.LogAsync(
                "PromoCode", "Toggle",
                _userContextService.UserId, _userContextService.Email,
                $"Toggled promo code '{entity.Code}' IsActive to {entity.IsActive}",
                entityId: entity.Id.ToString(),
                ipAddress: _userContextService.IpAddress,
                userAgent: _userContextService.UserAgent,
                oldValues: oldValues,
                newValues: JsonSerializer.Serialize(new { entity.IsActive }));

            return Result<bool>.Ok(true, $"Promo code is now {(entity.IsActive ? "active" : "inactive")}.");
        }

        public async Task<Result<bool>> DeleteAsync(int id)
        {
            var entity = await _unitOfWork.Repository<PromoCode>().GetByIdAsync(id);
            if (entity == null)
                return Result<bool>.Fail("Promo code not found.");

            if (entity.UsageCount > 0)
                return Result<bool>.Fail("Cannot delete a promo code that has already been used.");

            var oldValues = JsonSerializer.Serialize(new
            {
                entity.Code,
                entity.DiscountPercent,
                entity.MaxUsages,
                entity.UsageCount,
                entity.ExpiresAt,
                entity.IsActive
            });

            _unitOfWork.Repository<PromoCode>().Remove(entity);
            await _unitOfWork.SaveChangesAsync();

            await _auditLogService.LogAsync(
                "PromoCode", "Delete",
                _userContextService.UserId, _userContextService.Email,
                $"Deleted promo code '{entity.Code}'",
                entityId: id.ToString(),
                ipAddress: _userContextService.IpAddress,
                userAgent: _userContextService.UserAgent,
                oldValues: oldValues,
                newValues: null);

            return Result<bool>.Ok(true, "Promo code deleted successfully.");
        }
    }
}
