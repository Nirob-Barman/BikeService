using System.Text.Json;
using BikeService.Application.DTOs.PaymentGateway;
using BikeService.Application.Interfaces;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Mappers;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;
using MediatR;

namespace BikeService.Application.Features.PaymentGateways.Commands.CreatePaymentGateway;

public class CreatePaymentGatewayCommandHandler(
    IUnitOfWork unitOfWork,
    IAuditLogService auditLogService,
    IUserContextService userContextService,
    IConfigEncryptor configEncryptor) : IRequestHandler<CreatePaymentGatewayCommand, Result<int>>
{
    public async Task<Result<int>> Handle(CreatePaymentGatewayCommand request, CancellationToken cancellationToken)
    {
        var duplicate = await unitOfWork.Repository<PaymentGateway>()
            .AnyAsync(g => g.Slug == request.Slug);
        if (duplicate)
            return Result<int>.FailField("Slug", "A gateway with this slug already exists.");

        string encryptedConfig;
        try
        {
            encryptedConfig = configEncryptor.Encrypt(request.Config);
        }
        catch
        {
            return Result<int>.Fail("Failed to encrypt gateway configuration.");
        }

        var entity = PaymentGatewayMapper.ToEntity(new PaymentGatewayFormDto
        {
            Slug = request.Slug,
            Name = request.Name,
            Config = request.Config,
            IsActive = request.IsActive,
            IsSandbox = request.IsSandbox
        });
        entity.Config = encryptedConfig;
        entity.CreatedBy = userContextService.UserId;
        entity.CreatedAt = DateTime.UtcNow;

        await unitOfWork.Repository<PaymentGateway>().AddAsync(entity);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await auditLogService.LogAsync(
            "PaymentGateway", "Create",
            userContextService.UserId, userContextService.Email,
            $"Created payment gateway '{entity.Name}' (slug: {entity.Slug})",
            entityId: entity.Id.ToString(),
            ipAddress: userContextService.IpAddress,
            userAgent: userContextService.UserAgent,
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
}
