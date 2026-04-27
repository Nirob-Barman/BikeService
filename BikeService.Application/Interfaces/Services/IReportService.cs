using BikeService.Application.DTOs.Report;
using BikeService.Application.Wrappers;

namespace BikeService.Application.Interfaces.Services
{
    public interface IReportService
    {
        Task<Result<RevenueReportDto>> GetRevenueReportAsync(ReportFilterDto filter);
        Task<Result<TicketReportDto>> GetTicketReportAsync(ReportFilterDto filter);
        Task<Result<List<PartUsageReportDto>>> GetPartUsageReportAsync(ReportFilterDto filter);
        Task<string> ExportInvoicesCsvAsync(ReportFilterDto filter);
        Task<string> ExportTicketsCsvAsync(ReportFilterDto filter);
        Task<string> ExportPartUsageCsvAsync(ReportFilterDto filter);
    }
}
