using System.Text.Json;
using BikeService.Application.Interfaces;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;
using MediatR;

namespace BikeService.Application.Features.PromoCodes.Commands.CreatePromoCode;

public class CreatePromoCodeCommandHandler(
    IUnitOfWork unitOfWork,
    IAuditLogService auditLogService,
    IUserContextService userContextService) : IRequestHandler<CreatePromoCodeCommand, Result<int>>
{
    public async Task<Result<int>> Handle(CreatePromoCodeCommand request, CancellationToken cancellationToken)
    {
        var duplicate = await unitOfWork.Repository<PromoCode>()
            .AnyAsync(e => e.Code == request.Code);
        if (duplicate)
            return Result<int>.FailField("Code", "A promo code with this code already exists.");

        var entity = new PromoCode
        {
            Code = request.Code,
            DiscountPercent = request.DiscountPercent,
            MaxUsages = request.MaxUsages,
            ExpiresAt = request.ExpiresAt,
            IsActive = request.IsActive,
            CreatedBy = userContextService.UserId,
            CreatedAt = DateTime.UtcNow
        };

        await unitOfWork.Repository<PromoCode>().AddAsync(entity);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await auditLogService.LogAsync(
            "PromoCode", "Create",
            userContextService.UserId, userContextService.Email,
            $"Created promo code '{entity.Code}'",
            entityId: entity.Id.ToString(),
            ipAddress: userContextService.IpAddress,
            userAgent: userContextService.UserAgent,
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
}
