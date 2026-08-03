using System.Text.Json;
using BikeService.Application.DTOs.PaymentGateway;
using BikeService.Application.Interfaces;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Mappers;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;
using MediatR;

namespace BikeService.Application.Features.PaymentGateways.Commands.UpdatePaymentGateway;

public class UpdatePaymentGatewayCommandHandler(
    IUnitOfWork unitOfWork,
    IAuditLogService auditLogService,
    IUserContextService userContextService,
    IConfigEncryptor configEncryptor) : IRequestHandler<UpdatePaymentGatewayCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(UpdatePaymentGatewayCommand request, CancellationToken cancellationToken)
    {
        var entity = await unitOfWork.Repository<PaymentGateway>().GetByIdAsync(request.Id);
        if (entity == null)
            return Result<bool>.Fail("Payment gateway not found.");

        var duplicate = await unitOfWork.Repository<PaymentGateway>()
            .AnyAsync(g => g.Slug == request.Slug && g.Id != request.Id);
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
        if (!string.IsNullOrWhiteSpace(request.Config))
        {
            try
            {
                var existing = new Dictionary<string, string>();
                try
                {
                    var decrypted = configEncryptor.Decrypt(entity.Config);
                    existing = JsonSerializer.Deserialize<Dictionary<string, string>>(decrypted)
                               ?? new Dictionary<string, string>();
                }
                catch { /* existing config unreadable — start fresh */ }

                var incoming = JsonSerializer.Deserialize<Dictionary<string, string>>(request.Config)
                               ?? new Dictionary<string, string>();

                foreach (var kv in incoming)
                    existing[kv.Key] = kv.Value;

                entity.Config = configEncryptor.Encrypt(JsonSerializer.Serialize(existing));
            }
            catch
            {
                return Result<bool>.Fail("Failed to encrypt gateway configuration.");
            }
        }
        // else: keep existing encrypted Config untouched

        PaymentGatewayMapper.UpdateEntity(entity, new PaymentGatewayFormDto
        {
            Slug = request.Slug,
            Name = request.Name,
            Config = request.Config,
            IsActive = request.IsActive,
            IsSandbox = request.IsSandbox
        });
        entity.UpdatedBy = userContextService.UserId;
        entity.UpdatedAt = DateTime.UtcNow;

        unitOfWork.Repository<PaymentGateway>().Update(entity);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await auditLogService.LogAsync(
            "PaymentGateway", "Update",
            userContextService.UserId, userContextService.Email,
            $"Updated payment gateway '{entity.Name}' (slug: {entity.Slug})",
            entityId: request.Id.ToString(),
            ipAddress: userContextService.IpAddress,
            userAgent: userContextService.UserAgent,
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
}
