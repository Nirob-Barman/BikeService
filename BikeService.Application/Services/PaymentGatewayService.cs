using System.Text.Json;
using BikeService.Application.DTOs.PaymentGateway;
using BikeService.Application.Interfaces;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Interfaces.Services;
using BikeService.Application.Mappers;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;

namespace BikeService.Application.Services
{
    public class PaymentGatewayService : IPaymentGatewayService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditLogService _auditLogService;
        private readonly IUserContextService _userContextService;
        private readonly IConfigEncryptor _configEncryptor;

        public PaymentGatewayService(
            IUnitOfWork unitOfWork,
            IAuditLogService auditLogService,
            IUserContextService userContextService,
            IConfigEncryptor configEncryptor)
        {
            _unitOfWork = unitOfWork;
            _auditLogService = auditLogService;
            _userContextService = userContextService;
            _configEncryptor = configEncryptor;
        }

        public async Task<Result<List<PaymentGatewayDto>>> GetAllAsync()
        {
            var gateways = await _unitOfWork.Repository<PaymentGateway>()
                .GetAllWithIncludesAsync<PaymentGateway>(
                    g => g,
                    g => g.Transactions);

            var dtos = gateways.Select(PaymentGatewayMapper.ToDto).ToList();
            return Result<List<PaymentGatewayDto>>.Ok(dtos);
        }

        public async Task<Result<PaymentGatewayDto>> GetByIdAsync(int id)
        {
            var gateways = await _unitOfWork.Repository<PaymentGateway>()
                .GetAllWithIncludesAsync<PaymentGateway>(
                    g => g.Id == id,
                    g => g,
                    g => g.Transactions);

            var gateway = gateways.FirstOrDefault();
            if (gateway == null)
                return Result<PaymentGatewayDto>.Fail("Payment gateway not found.");

            return Result<PaymentGatewayDto>.Ok(PaymentGatewayMapper.ToDto(gateway));
        }

        public async Task<Result<string>> GetDecryptedConfigAsync(int id)
        {
            var gateway = await _unitOfWork.Repository<PaymentGateway>().GetByIdAsync(id);
            if (gateway == null)
                return Result<string>.Fail("Payment gateway not found.");

            try
            {
                var decrypted = _configEncryptor.Decrypt(gateway.Config);
                return Result<string>.Ok(decrypted);
            }
            catch
            {
                return Result<string>.Fail("Failed to decrypt gateway configuration.");
            }
        }

        public async Task<Result<int>> CreateAsync(PaymentGatewayFormDto dto)
        {
            var duplicate = await _unitOfWork.Repository<PaymentGateway>()
                .AnyAsync(g => g.Slug == dto.Slug);
            if (duplicate)
                return Result<int>.FailField("Slug", "A gateway with this slug already exists.");

            string encryptedConfig;
            try
            {
                encryptedConfig = _configEncryptor.Encrypt(dto.Config);
            }
            catch
            {
                return Result<int>.Fail("Failed to encrypt gateway configuration.");
            }

            var entity = PaymentGatewayMapper.ToEntity(dto);
            entity.Config = encryptedConfig;
            entity.CreatedBy = _userContextService.UserId;
            entity.CreatedAt = DateTime.UtcNow;

            await _unitOfWork.Repository<PaymentGateway>().AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            await _auditLogService.LogAsync(
                "PaymentGateway", "Create",
                _userContextService.UserId, _userContextService.Email,
                $"Created payment gateway '{entity.Name}' (slug: {entity.Slug})",
                entityId: entity.Id.ToString(),
                ipAddress: _userContextService.IpAddress,
                userAgent: _userContextService.UserAgent,
                oldValues: null,
                newValues: JsonSerializer.Serialize(new
                {
                    entity.Slug,
                    entity.Name,
                    entity.IsActive,
                    entity.IsSandbox
                }));

            return Result<int>.Ok(entity.Id, "Payment gateway created successfully.");
        }

        public async Task<Result<bool>> UpdateAsync(int id, PaymentGatewayFormDto dto)
        {
            var entity = await _unitOfWork.Repository<PaymentGateway>().GetByIdAsync(id);
            if (entity == null)
                return Result<bool>.Fail("Payment gateway not found.");

            var duplicate = await _unitOfWork.Repository<PaymentGateway>()
                .AnyAsync(g => g.Slug == dto.Slug && g.Id != id);
            if (duplicate)
                return Result<bool>.FailField("Slug", "A gateway with this slug already exists.");

            var oldValues = JsonSerializer.Serialize(new
            {
                entity.Slug,
                entity.Name,
                entity.IsActive,
                entity.IsSandbox
            });

            // Merge incoming config fields on top of existing encrypted config.
            // BuildConfig omits blank fields, so missing keys = keep existing value.
            if (!string.IsNullOrWhiteSpace(dto.Config))
            {
                try
                {
                    var existing = new Dictionary<string, string>();
                    try
                    {
                        var decrypted = _configEncryptor.Decrypt(entity.Config);
                        existing = JsonSerializer.Deserialize<Dictionary<string, string>>(decrypted)
                                   ?? new Dictionary<string, string>();
                    }
                    catch { /* existing config unreadable — start fresh */ }

                    var incoming = JsonSerializer.Deserialize<Dictionary<string, string>>(dto.Config)
                                   ?? new Dictionary<string, string>();

                    foreach (var kv in incoming)
                        existing[kv.Key] = kv.Value;

                    entity.Config = _configEncryptor.Encrypt(JsonSerializer.Serialize(existing));
                }
                catch
                {
                    return Result<bool>.Fail("Failed to encrypt gateway configuration.");
                }
            }
            // else: keep existing encrypted Config untouched

            PaymentGatewayMapper.UpdateEntity(entity, dto);
            entity.UpdatedBy = _userContextService.UserId;
            entity.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Repository<PaymentGateway>().Update(entity);
            await _unitOfWork.SaveChangesAsync();

            await _auditLogService.LogAsync(
                "PaymentGateway", "Update",
                _userContextService.UserId, _userContextService.Email,
                $"Updated payment gateway '{entity.Name}' (slug: {entity.Slug})",
                entityId: id.ToString(),
                ipAddress: _userContextService.IpAddress,
                userAgent: _userContextService.UserAgent,
                oldValues: oldValues,
                newValues: JsonSerializer.Serialize(new
                {
                    entity.Slug,
                    entity.Name,
                    entity.IsActive,
                    entity.IsSandbox
                }));

            return Result<bool>.Ok(true, "Payment gateway updated successfully.");
        }

        public async Task<Result<bool>> ToggleActiveAsync(int id)
        {
            var entity = await _unitOfWork.Repository<PaymentGateway>().GetByIdAsync(id);
            if (entity == null)
                return Result<bool>.Fail("Payment gateway not found.");

            var oldValues = JsonSerializer.Serialize(new { entity.IsActive });

            entity.IsActive = !entity.IsActive;
            entity.UpdatedBy = _userContextService.UserId;
            entity.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Repository<PaymentGateway>().Update(entity);
            await _unitOfWork.SaveChangesAsync();

            await _auditLogService.LogAsync(
                "PaymentGateway", "Toggle",
                _userContextService.UserId, _userContextService.Email,
                $"Toggled payment gateway '{entity.Name}' IsActive to {entity.IsActive}",
                entityId: id.ToString(),
                ipAddress: _userContextService.IpAddress,
                userAgent: _userContextService.UserAgent,
                oldValues: oldValues,
                newValues: JsonSerializer.Serialize(new { entity.IsActive }));

            return Result<bool>.Ok(true, $"Gateway is now {(entity.IsActive ? "active" : "inactive")}.");
        }

        public async Task<Result<bool>> DeleteAsync(int id)
        {
            var entity = await _unitOfWork.Repository<PaymentGateway>().GetByIdAsync(id);
            if (entity == null)
                return Result<bool>.Fail("Payment gateway not found.");

            var hasTransactions = await _unitOfWork.Repository<PaymentTransaction>()
                .AnyAsync(t => t.GatewayId == id);
            if (hasTransactions)
                return Result<bool>.Fail("Cannot delete this gateway because it has associated transactions.");

            var oldValues = JsonSerializer.Serialize(new
            {
                entity.Slug,
                entity.Name,
                entity.IsActive,
                entity.IsSandbox
            });

            _unitOfWork.Repository<PaymentGateway>().Remove(entity);
            await _unitOfWork.SaveChangesAsync();

            await _auditLogService.LogAsync(
                "PaymentGateway", "Delete",
                _userContextService.UserId, _userContextService.Email,
                $"Deleted payment gateway '{entity.Name}' (slug: {entity.Slug})",
                entityId: id.ToString(),
                ipAddress: _userContextService.IpAddress,
                userAgent: _userContextService.UserAgent,
                oldValues: oldValues,
                newValues: null);

            return Result<bool>.Ok(true, "Payment gateway deleted successfully.");
        }
    }
}
