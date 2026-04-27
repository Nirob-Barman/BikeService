using BikeService.Application.DTOs.Invoice;
using BikeService.Application.Wrappers;

namespace BikeService.Application.Interfaces.Services
{
    public interface IInvoiceService
    {
        Task<Result<List<InvoiceDto>>> GetAllAsync(InvoiceFilterDto? filter = null);
        Task<Result<InvoiceDto>> GetByIdAsync(int id);
        Task<Result<InvoiceDto>> GetByTicketIdAsync(int ticketId);
        Task<Result<int>> GenerateAsync(int ticketId);
        Task<Result<List<InvoiceDto>>> GetMyInvoicesAsync();
        Task<Result<InvoiceDto>> GetMyInvoiceByIdAsync(int id);
        Task<Result<bool>> IssueAsync(int id);
        Task<Result<bool>> VoidAsync(int id);
    }
}
