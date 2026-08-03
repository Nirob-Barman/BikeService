using System.Text.Json;
using BikeService.Application.DTOs.Part;
using BikeService.Application.Interfaces;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Mappers;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BikeService.Application.Features.Parts.Commands.UpdatePart;

public class UpdatePartCommandHandler(
    IUnitOfWork unitOfWork,
    IAuditLogService auditLogService,
    IUserContextService userContextService,
    IEmailService emailService,
    ILogger<UpdatePartCommandHandler> logger) : IRequestHandler<UpdatePartCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(UpdatePartCommand request, CancellationToken cancellationToken)
    {
        var part = await unitOfWork.Repository<Part>().GetByIdAsync(request.Id);
        if (part is null)
            return Result<bool>.Fail("Part not found.");

        var skuExists = await unitOfWork.Repository<Part>().AnyAsync(p => p.SKU == request.SKU && p.Id != request.Id);
        if (skuExists)
            return Result<bool>.FailField("SKU", "Another part with this SKU already exists.");

        var oldValues = JsonSerializer.Serialize(new { part.Name, part.SKU, part.UnitPrice, part.StockQuantity, part.LowStockThreshold });

        var dto = new PartFormDto
        {
            Name = request.Name,
            SKU = request.SKU,
            UnitPrice = request.UnitPrice,
            StockQuantity = request.StockQuantity,
            LowStockThreshold = request.LowStockThreshold
        };

        PartMapper.UpdateEntity(part, dto);
        part.UpdatedAt = DateTime.UtcNow;
        part.UpdatedBy = userContextService.UserId;

        unitOfWork.Repository<Part>().Update(part);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await auditLogService.LogAsync(
            "Part", "Update",
            userContextService.UserId, userContextService.Email,
            $"Updated part '{part.Name}' (SKU: {part.SKU})",
            entityId: part.Id.ToString(),
            ipAddress: userContextService.IpAddress,
            userAgent: userContextService.UserAgent,
            oldValues: oldValues,
            newValues: JsonSerializer.Serialize(new { part.Name, part.SKU, part.UnitPrice, part.StockQuantity, part.LowStockThreshold }));

        await CheckAndCreateStockAlertAsync(part);

        return Result<bool>.Ok(true, "Part updated successfully.");
    }

    private async Task CheckAndCreateStockAlertAsync(Part part)
    {
        if (part.StockQuantity > part.LowStockThreshold)
            return;

        var unresolvedExists = await unitOfWork.Repository<PartStockAlert>()
            .AnyAsync(a => a.PartId == part.Id && !a.IsResolved);

        if (unresolvedExists)
            return;

        var alert = new PartStockAlert
        {
            PartId = part.Id,
            IsResolved = false,
            CreatedBy = userContextService.UserId
        };

        await unitOfWork.Repository<PartStockAlert>().AddAsync(alert);
        await unitOfWork.SaveChangesAsync();

        try
        {
            await emailService.SendEmailAsync(
                subject: $"Low Stock Alert: {part.Name}",
                message: $"<p>The part <strong>{part.Name}</strong> (SKU: {part.SKU}) has fallen below its low stock threshold.</p>" +
                         $"<p>Current stock: <strong>{part.StockQuantity}</strong> | Threshold: <strong>{part.LowStockThreshold}</strong></p>" +
                         $"<p>Please restock this item at your earliest convenience.</p>",
                toEmails: new List<string> { userContextService.Email ?? "admin@bikeservice.com" });
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to send low stock alert email for part {PartId} ({PartName}).", part.Id, part.Name);
        }
    }
}
