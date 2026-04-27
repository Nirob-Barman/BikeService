using System.Text.Json;
using BikeService.Application.DTOs.Part;
using BikeService.Application.Interfaces;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Interfaces.Services;
using BikeService.Application.Mappers;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace BikeService.Application.Services
{
    public class PartService : IPartService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditLogService _auditLogService;
        private readonly IUserContextService _userContextService;
        private readonly IEmailService _emailService;
        private readonly INotificationService _notificationService;
        private readonly ILogger<PartService> _logger;

        public PartService(
            IUnitOfWork unitOfWork,
            IAuditLogService auditLogService,
            IUserContextService userContextService,
            IEmailService emailService,
            INotificationService notificationService,
            ILogger<PartService> logger)
        {
            _unitOfWork = unitOfWork;
            _auditLogService = auditLogService;
            _userContextService = userContextService;
            _emailService = emailService;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<Result<List<PartDto>>> GetAllAsync()
        {
            var parts = await _unitOfWork.Repository<Part>().GetAllAsync();
            var dtos = parts.Select(PartMapper.ToDto).ToList();
            return Result<List<PartDto>>.Ok(dtos);
        }

        public async Task<Result<PartDto>> GetByIdAsync(int id)
        {
            var part = await _unitOfWork.Repository<Part>().GetByIdAsync(id);
            if (part is null)
                return Result<PartDto>.Fail("Part not found.");

            return Result<PartDto>.Ok(PartMapper.ToDto(part));
        }

        public async Task<Result<int>> CreateAsync(PartFormDto dto)
        {
            var skuExists = await _unitOfWork.Repository<Part>().AnyAsync(p => p.SKU == dto.SKU);
            if (skuExists)
                return Result<int>.FailField("SKU", "A part with this SKU already exists.");

            var part = PartMapper.ToEntity(dto);
            part.CreatedBy = _userContextService.UserId;

            await _unitOfWork.Repository<Part>().AddAsync(part);
            await _unitOfWork.SaveChangesAsync();

            await _auditLogService.LogAsync(
                "Part", "Create",
                _userContextService.UserId, _userContextService.Email,
                $"Created part '{part.Name}' (SKU: {part.SKU})",
                entityId: part.Id.ToString(),
                ipAddress: _userContextService.IpAddress,
                userAgent: _userContextService.UserAgent,
                newValues: JsonSerializer.Serialize(new { part.Name, part.SKU, part.UnitPrice, part.StockQuantity, part.LowStockThreshold }));

            await CheckAndCreateStockAlertAsync(part);

            return Result<int>.Ok(part.Id, "Part created successfully.");
        }

        public async Task<Result<bool>> UpdateAsync(int id, PartFormDto dto)
        {
            var part = await _unitOfWork.Repository<Part>().GetByIdAsync(id);
            if (part is null)
                return Result<bool>.Fail("Part not found.");

            var skuExists = await _unitOfWork.Repository<Part>().AnyAsync(p => p.SKU == dto.SKU && p.Id != id);
            if (skuExists)
                return Result<bool>.FailField("SKU", "Another part with this SKU already exists.");

            var oldValues = JsonSerializer.Serialize(new { part.Name, part.SKU, part.UnitPrice, part.StockQuantity, part.LowStockThreshold });

            PartMapper.UpdateEntity(part, dto);
            part.UpdatedAt = DateTime.UtcNow;
            part.UpdatedBy = _userContextService.UserId;

            _unitOfWork.Repository<Part>().Update(part);
            await _unitOfWork.SaveChangesAsync();

            await _auditLogService.LogAsync(
                "Part", "Update",
                _userContextService.UserId, _userContextService.Email,
                $"Updated part '{part.Name}' (SKU: {part.SKU})",
                entityId: part.Id.ToString(),
                ipAddress: _userContextService.IpAddress,
                userAgent: _userContextService.UserAgent,
                oldValues: oldValues,
                newValues: JsonSerializer.Serialize(new { part.Name, part.SKU, part.UnitPrice, part.StockQuantity, part.LowStockThreshold }));

            await CheckAndCreateStockAlertAsync(part);

            return Result<bool>.Ok(true, "Part updated successfully.");
        }

        public async Task<Result<bool>> DeleteAsync(int id)
        {
            var part = await _unitOfWork.Repository<Part>().GetByIdAsync(id);
            if (part is null)
                return Result<bool>.Fail("Part not found.");

            var hasTicketItems = await _unitOfWork.Repository<ServiceTicketItem>().AnyAsync(i => i.PartId == id);
            if (hasTicketItems)
                return Result<bool>.Fail("Cannot delete part because it is referenced in one or more service ticket items.");

            var oldValues = JsonSerializer.Serialize(new { part.Name, part.SKU, part.UnitPrice, part.StockQuantity, part.LowStockThreshold });

            // Remove unresolved alerts for this part first
            var unresolvedAlerts = await _unitOfWork.Repository<PartStockAlert>().Where(a => a.PartId == id && !a.IsResolved);
            if (unresolvedAlerts.Any())
                _unitOfWork.Repository<PartStockAlert>().RemoveRange(unresolvedAlerts);

            _unitOfWork.Repository<Part>().Remove(part);
            await _unitOfWork.SaveChangesAsync();

            await _auditLogService.LogAsync(
                "Part", "Delete",
                _userContextService.UserId, _userContextService.Email,
                $"Deleted part '{part.Name}' (SKU: {part.SKU})",
                entityId: id.ToString(),
                ipAddress: _userContextService.IpAddress,
                userAgent: _userContextService.UserAgent,
                oldValues: oldValues);

            return Result<bool>.Ok(true, "Part deleted successfully.");
        }

        public async Task<Result<List<PartStockAlertDto>>> GetStockAlertsAsync(bool unresolvedOnly = true)
        {
            IEnumerable<PartStockAlert> alerts;

            if (unresolvedOnly)
                alerts = await _unitOfWork.Repository<PartStockAlert>().Where(a => !a.IsResolved);
            else
                alerts = await _unitOfWork.Repository<PartStockAlert>().GetAllAsync();

            var partIds = alerts.Select(a => a.PartId).Distinct().ToList();
            var parts = await _unitOfWork.Repository<Part>().Where(p => partIds.Contains(p.Id));
            var partDict = parts.ToDictionary(p => p.Id);

            var dtos = alerts
                .Where(a => partDict.ContainsKey(a.PartId))
                .Select(a => PartMapper.ToStockAlertDto(a, partDict[a.PartId]))
                .ToList();

            return Result<List<PartStockAlertDto>>.Ok(dtos);
        }

        public async Task<Result<bool>> ResolveStockAlertAsync(int alertId)
        {
            var alert = await _unitOfWork.Repository<PartStockAlert>().GetByIdAsync(alertId);
            if (alert is null)
                return Result<bool>.Fail("Stock alert not found.");

            if (alert.IsResolved)
                return Result<bool>.Fail("Stock alert is already resolved.");

            alert.IsResolved = true;
            alert.UpdatedAt = DateTime.UtcNow;
            alert.UpdatedBy = _userContextService.UserId;

            _unitOfWork.Repository<PartStockAlert>().Update(alert);
            await _unitOfWork.SaveChangesAsync();

            return Result<bool>.Ok(true, "Stock alert resolved.");
        }

        private async Task CheckAndCreateStockAlertAsync(Part part)
        {
            if (part.StockQuantity > part.LowStockThreshold)
                return;

            var unresolvedExists = await _unitOfWork.Repository<PartStockAlert>()
                .AnyAsync(a => a.PartId == part.Id && !a.IsResolved);

            if (unresolvedExists)
                return;

            var alert = new PartStockAlert
            {
                PartId = part.Id,
                IsResolved = false,
                CreatedBy = _userContextService.UserId
            };

            await _unitOfWork.Repository<PartStockAlert>().AddAsync(alert);
            await _unitOfWork.SaveChangesAsync();

            try
            {
                await _emailService.SendEmailAsync(
                    subject: $"Low Stock Alert: {part.Name}",
                    message: $"<p>The part <strong>{part.Name}</strong> (SKU: {part.SKU}) has fallen below its low stock threshold.</p>" +
                             $"<p>Current stock: <strong>{part.StockQuantity}</strong> | Threshold: <strong>{part.LowStockThreshold}</strong></p>" +
                             $"<p>Please restock this item at your earliest convenience.</p>",
                    toEmails: new List<string> { _userContextService.Email ?? "admin@bikeservice.com" });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send low stock alert email for part {PartId} ({PartName}).", part.Id, part.Name);
            }
        }
    }
}
