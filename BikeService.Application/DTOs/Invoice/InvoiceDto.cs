using BikeService.Application.DTOs.ServiceTicket;
using BikeService.Domain.Enums;

namespace BikeService.Application.DTOs.Invoice
{
    public class InvoiceDto
    {
        public int Id { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal FinalAmount { get; set; }
        public InvoiceStatus Status { get; set; }
        public string StatusLabel => Status.ToString();

        public int ServiceTicketId { get; set; }
        public string BikeSummary { get; set; } = string.Empty;
        public string? CustomerName { get; set; }

        public int? PromoCodeId { get; set; }
        public string? PromoCode { get; set; }

        public DateTime CreatedAt { get; set; }

        public List<ServiceTicketItemDto> Items { get; set; } = new();
        public List<PaymentTransactionDto> PaymentTransactions { get; set; } = new();
    }
}
