using BikeService.Application.DTOs.Invoice;
using BikeService.Application.DTOs.ServiceTicket;
using BikeService.Domain.Entities;

namespace BikeService.Application.Mappers
{
    public static class InvoiceMapper
    {
        public static InvoiceDto ToDto(Invoice e) => new()
        {
            Id = e.Id,
            TotalAmount = e.TotalAmount,
            TaxAmount = e.TaxAmount,
            DiscountAmount = e.DiscountAmount,
            FinalAmount = e.FinalAmount,
            Status = e.Status,
            ServiceTicketId = e.ServiceTicketId,
            BikeSummary = e.ServiceTicket?.Bike != null
                ? $"{e.ServiceTicket.Bike.Year} {e.ServiceTicket.Bike.Make} {e.ServiceTicket.Bike.Model}"
                : string.Empty,
            PromoCodeId = e.PromoCodeId,
            PromoCode = e.PromoCode?.Code,
            CreatedAt = e.CreatedAt,
            Items = e.ServiceTicket?.Items?
                .Select(i => new ServiceTicketItemDto
                {
                    Id = i.Id,
                    ServiceTicketId = i.ServiceTicketId,
                    ServiceTypeId = i.ServiceTypeId,
                    ServiceTypeName = i.ServiceType?.Name,
                    PartId = i.PartId,
                    PartName = i.Part?.Name,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice
                }).ToList() ?? new(),
            PaymentTransactions = e.PaymentTransactions?
                .Select(TransactionToDto)
                .ToList() ?? new()
        };

        public static PaymentTransactionDto TransactionToDto(PaymentTransaction t) => new()
        {
            Id = t.Id,
            Amount = t.Amount,
            SessionRef = t.SessionRef,
            Status = t.Status,
            GatewayName = t.Gateway?.Name ?? string.Empty,
            CreatedAt = t.CreatedAt
        };
    }
}
