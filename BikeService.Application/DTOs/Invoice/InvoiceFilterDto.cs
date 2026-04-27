using BikeService.Domain.Enums;

namespace BikeService.Application.DTOs.Invoice
{
    public class InvoiceFilterDto
    {
        public InvoiceStatus? Status { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
    }
}
