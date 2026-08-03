using System.Text.Json;
using BikeService.Application.Interfaces;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;
using MediatR;

namespace BikeService.Application.Features.PaymentGateways.Commands.DeletePaymentGateway;

public class DeletePaymentGatewayCommandHandler(
    IUnitOfWork unitOfWork,
    IAuditLogService auditLogService,
    IUserContextService userContextService) : IRequestHandler<DeletePaymentGatewayCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(DeletePaymentGatewayCommand request, CancellationToken cancellationToken)
    {
        var entity = await unitOfWork.Repository<PaymentGateway>().GetByIdAsync(request.Id);
        if (entity == null)
            return Result<bool>.Fail("Payment gateway not found.");

        var hasTransactions = await unitOfWork.Repository<PaymentTransaction>()
            .AnyAsync(t => t.GatewayId == request.Id);
        if (hasTransactions)
            return Result<bool>.Fail("Cannot delete this gateway because it has associated transactions.");

        var oldValues = JsonSerializer.Serialize(new
        {
            entity.Slug,
            entity.Name,
            entity.IsActive,
            entity.IsSandbox
        });

        unitOfWork.Repository<PaymentGateway>().Remove(entity);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await auditLogService.LogAsync(
            "PaymentGateway", "Delete",
            userContextService.UserId, userContextService.Email,
            $"Deleted payment gateway '{entity.Name}' (slug: {entity.Slug})",
            entityId: request.Id.ToString(),
            ipAddress: userContextService.IpAddress,
            userAgent: userContextService.UserAgent,
            oldValues: oldValues,
            newValues: null);

        return Result<bool>.Ok(true, "Payment gateway deleted successfully.");
    }
}
