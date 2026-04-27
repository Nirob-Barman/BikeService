using BikeService.Application.DTOs.Invoice;

namespace BikeService.Application.Interfaces
{
    public interface IPdfService
    {
        byte[] GenerateInvoicePdf(InvoiceDto invoice);
    }
}
