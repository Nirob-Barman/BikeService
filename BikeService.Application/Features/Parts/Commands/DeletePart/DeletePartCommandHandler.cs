using System.Text.Json;
using BikeService.Application.Interfaces;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;
using MediatR;

namespace BikeService.Application.Features.Parts.Commands.DeletePart;

public class DeletePartCommandHandler(
    IUnitOfWork unitOfWork,
    IAuditLogService auditLogService,
    IUserContextService userContextService) : IRequestHandler<DeletePartCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(DeletePartCommand request, CancellationToken cancellationToken)
    {
        var part = await unitOfWork.Repository<Part>().GetByIdAsync(request.Id);
        if (part is null)
            return Result<bool>.Fail("Part not found.");

        var hasTicketItems = await unitOfWork.Repository<ServiceTicketItem>().AnyAsync(i => i.PartId == request.Id);
        if (hasTicketItems)
            return Result<bool>.Fail("Cannot delete part because it is referenced in one or more service ticket items.");

        var oldValues = JsonSerializer.Serialize(new { part.Name, part.SKU, part.UnitPrice, part.StockQuantity, part.LowStockThreshold });

        // Remove unresolved alerts for this part first
        var unresolvedAlerts = await unitOfWork.Repository<PartStockAlert>().Where(a => a.PartId == request.Id && !a.IsResolved);
        if (unresolvedAlerts.Any())
            unitOfWork.Repository<PartStockAlert>().RemoveRange(unresolvedAlerts);

        unitOfWork.Repository<Part>().Remove(part);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await auditLogService.LogAsync(
            "Part", "Delete",
            userContextService.UserId, userContextService.Email,
            $"Deleted part '{part.Name}' (SKU: {part.SKU})",
            entityId: request.Id.ToString(),
            ipAddress: userContextService.IpAddress,
            userAgent: userContextService.UserAgent,
            oldValues: oldValues);

        return Result<bool>.Ok(true, "Part deleted successfully.");
    }
}
