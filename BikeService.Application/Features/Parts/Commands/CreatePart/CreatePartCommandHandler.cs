using System.Text.Json;
using BikeService.Application.DTOs.Part;
using BikeService.Application.Interfaces;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Mappers;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BikeService.Application.Features.Parts.Commands.CreatePart;

public class CreatePartCommandHandler(
    IUnitOfWork unitOfWork,
    IAuditLogService auditLogService,
    IUserContextService userContextService,
    IEmailService emailService,
    ILogger<CreatePartCommandHandler> logger) : IRequestHandler<CreatePartCommand, Result<int>>
{
    public async Task<Result<int>> Handle(CreatePartCommand request, CancellationToken cancellationToken)
    {
        var skuExists = await unitOfWork.Repository<Part>().AnyAsync(p => p.SKU == request.SKU);
        if (skuExists)
            return Result<int>.FailField("SKU", "A part with this SKU already exists.");

        var dto = new PartFormDto
        {
            Name = request.Name,
            SKU = request.SKU,
            UnitPrice = request.UnitPrice,
            StockQuantity = request.StockQuantity,
            LowStockThreshold = request.LowStockThreshold
        };

        var part = PartMapper.ToEntity(dto);
        part.CreatedBy = userContextService.UserId;

        await unitOfWork.Repository<Part>().AddAsync(part);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await auditLogService.LogAsync(
            "Part", "Create",
            userContextService.UserId, userContextService.Email,
            $"Created part '{part.Name}' (SKU: {part.SKU})",
            entityId: part.Id.ToString(),
            ipAddress: userContextService.IpAddress,
            userAgent: userContextService.UserAgent,
            newValues: JsonSerializer.Serialize(new { part.Name, part.SKU, part.UnitPrice, part.StockQuantity, part.LowStockThreshold }));

        await CheckAndCreateStockAlertAsync(part);

        return Result<int>.Ok(part.Id, "Part created successfully.");
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
