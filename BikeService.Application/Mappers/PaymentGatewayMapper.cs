using BikeService.Application.DTOs.PaymentGateway;
using BikeService.Domain.Entities;

namespace BikeService.Application.Mappers
{
    public static class PaymentGatewayMapper
    {
        public static PaymentGatewayDto ToDto(PaymentGateway e) => new()
        {
            Id = e.Id,
            Slug = e.Slug,
            Name = e.Name,
            IsActive = e.IsActive,
            IsSandbox = e.IsSandbox,
            CreatedAt = e.CreatedAt,
            TransactionCount = e.Transactions?.Count ?? 0
        };

        public static PaymentGateway ToEntity(PaymentGatewayFormDto dto) => new()
        {
            Slug = dto.Slug,
            Name = dto.Name,
            Config = dto.Config,
            IsActive = dto.IsActive,
            IsSandbox = dto.IsSandbox
        };

        public static void UpdateEntity(PaymentGateway e, PaymentGatewayFormDto dto)
        {
            e.Slug = dto.Slug;
            e.Name = dto.Name;
            e.IsActive = dto.IsActive;
            e.IsSandbox = dto.IsSandbox;
            // Config is handled separately in the service (merge logic)
        }
    }
}
