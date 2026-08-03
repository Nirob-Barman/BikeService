using System.Text.Json;
using BikeService.Application.Interfaces;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;
using MediatR;

namespace BikeService.Application.Features.PaymentGateways.Commands.TogglePaymentGatewayActive;

public class TogglePaymentGatewayActiveCommandHandler(
    IUnitOfWork unitOfWork,
    IAuditLogService auditLogService,
    IUserContextService userContextService) : IRequestHandler<TogglePaymentGatewayActiveCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(TogglePaymentGatewayActiveCommand request, CancellationToken cancellationToken)
    {
        var entity = await unitOfWork.Repository<PaymentGateway>().GetByIdAsync(request.Id);
        if (entity == null)
            return Result<bool>.Fail("Payment gateway not found.");

        var oldValues = JsonSerializer.Serialize(new { entity.IsActive });

        entity.IsActive = !entity.IsActive;
        entity.UpdatedBy = userContextService.UserId;
        entity.UpdatedAt = DateTime.UtcNow;

        unitOfWork.Repository<PaymentGateway>().Update(entity);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await auditLogService.LogAsync(
            "PaymentGateway", "Toggle",
            userContextService.UserId, userContextService.Email,
            $"Toggled payment gateway '{entity.Name}' IsActive to {entity.IsActive}",
            entityId: request.Id.ToString(),
            ipAddress: userContextService.IpAddress,
            userAgent: userContextService.UserAgent,
            oldValues: oldValues,
            newValues: JsonSerializer.Serialize(new { entity.IsActive }));

        return Result<bool>.Ok(true, $"Gateway is now {(entity.IsActive ? "active" : "inactive")}.");
    }
}
