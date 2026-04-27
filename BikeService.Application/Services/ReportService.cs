using System.Text;
using BikeService.Application.DTOs.Report;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Interfaces.Services;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;
using BikeService.Domain.Enums;

namespace BikeService.Application.Services
{
    public class ReportService : IReportService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ReportService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<RevenueReportDto>> GetRevenueReportAsync(ReportFilterDto filter)
        {
            var dateTo = filter.DateTo.Date.AddDays(1); // inclusive

            var invoices = await _unitOfWork.Repository<Invoice>()
                .Where(i => i.Status == InvoiceStatus.Paid
                         && i.CreatedAt >= filter.DateFrom.Date
                         && i.CreatedAt < dateTo);

            var invoiceList = invoices.ToList();

            // Revenue by service type
            var ticketIds = invoiceList.Select(i => i.ServiceTicketId).ToList();

            var items = await _unitOfWork.Repository<ServiceTicketItem>()
                .GetAllWithIncludesAsync<ServiceTicketItem>(
                    i => ticketIds.Contains(i.ServiceTicketId) && i.ServiceTypeId.HasValue,
                    i => i,
                    i => i.ServiceType!);

            var byService = items
                .GroupBy(i => i.ServiceType?.Name ?? "Unknown")
                .Select(g => new RevenueByServiceDto
                {
                    ServiceName = g.Key,
                    Quantity = g.Sum(i => i.Quantity),
                    Revenue = g.Sum(i => i.Quantity * i.UnitPrice)
                })
                .OrderByDescending(x => x.Revenue)
                .ToList();

            // Revenue by mechanic
            var tickets = await _unitOfWork.Repository<ServiceTicket>()
                .GetAllWithIncludesAsync<ServiceTicket>(
                    t => ticketIds.Contains(t.Id) && t.MechanicId.HasValue,
                    t => t,
                    t => t.Mechanic!);

            var ticketInvoiceMap = invoiceList.ToDictionary(i => i.ServiceTicketId, i => i.FinalAmount);

            var byMechanic = tickets
                .GroupBy(t => t.Mechanic?.FullName ?? "Unassigned")
                .Select(g => new RevenueByMechanicDto
                {
                    MechanicName = g.Key,
                    TicketsDelivered = g.Count(),
                    Revenue = g.Sum(t => ticketInvoiceMap.TryGetValue(t.Id, out var amt) ? amt : 0)
                })
                .OrderByDescending(x => x.Revenue)
                .ToList();

            var dto = new RevenueReportDto
            {
                TotalRevenue = invoiceList.Sum(i => i.FinalAmount),
                TotalTax = invoiceList.Sum(i => i.TaxAmount),
                TotalDiscount = invoiceList.Sum(i => i.DiscountAmount),
                InvoiceCount = invoiceList.Count,
                ByService = byService,
                ByMechanic = byMechanic
            };

            return Result<RevenueReportDto>.Ok(dto);
        }

        public async Task<Result<TicketReportDto>> GetTicketReportAsync(ReportFilterDto filter)
        {
            var dateTo = filter.DateTo.Date.AddDays(1);

            var tickets = await _unitOfWork.Repository<ServiceTicket>()
                .GetAllWithIncludesAsync<ServiceTicket>(
                    t => t.CreatedAt >= filter.DateFrom.Date && t.CreatedAt < dateTo,
                    t => t,
                    t => t.Mechanic!);

            var ticketList = tickets.ToList();

            var now = DateTime.UtcNow;

            var delivered = ticketList.Where(t => t.Status == ServiceTicketStatus.Delivered).ToList();
            var avgDays = delivered.Count > 0
                ? delivered.Average(t => (t.UpdatedAt ?? t.CreatedAt).Subtract(t.CreatedAt).TotalDays)
                : 0;

            var byStatus = ticketList
                .GroupBy(t => t.Status.ToString())
                .Select(g => new TicketsByStatusDto { Status = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToList();

            var byMechanic = ticketList
                .Where(t => t.MechanicId.HasValue)
                .GroupBy(t => t.Mechanic?.FullName ?? "Unknown")
                .Select(g => new TicketsByMechanicDto
                {
                    MechanicName = g.Key,
                    Total = g.Count(),
                    Delivered = g.Count(t => t.Status == ServiceTicketStatus.Delivered)
                })
                .OrderByDescending(x => x.Total)
                .ToList();

            var dto = new TicketReportDto
            {
                Total = ticketList.Count,
                Delivered = ticketList.Count(t => t.Status == ServiceTicketStatus.Delivered),
                Cancelled = ticketList.Count(t => t.Status == ServiceTicketStatus.Cancelled),
                Active = ticketList.Count(t => t.Status != ServiceTicketStatus.Delivered && t.Status != ServiceTicketStatus.Cancelled),
                Overdue = ticketList.Count(t =>
                    t.EstimatedCompletionDate.HasValue &&
                    t.EstimatedCompletionDate.Value < now &&
                    t.Status != ServiceTicketStatus.Delivered &&
                    t.Status != ServiceTicketStatus.Cancelled),
                AvgCompletionDays = Math.Round(avgDays, 1),
                ByStatus = byStatus,
                ByMechanic = byMechanic
            };

            return Result<TicketReportDto>.Ok(dto);
        }

        public async Task<Result<List<PartUsageReportDto>>> GetPartUsageReportAsync(ReportFilterDto filter)
        {
            var dateTo = filter.DateTo.Date.AddDays(1);

            // Get tickets created in range that reached InProgress or beyond
            var tickets = await _unitOfWork.Repository<ServiceTicket>()
                .Where(t => t.CreatedAt >= filter.DateFrom.Date
                         && t.CreatedAt < dateTo
                         && t.Status != ServiceTicketStatus.Pending
                         && t.Status != ServiceTicketStatus.Diagnosed);

            var ticketIds = tickets.Select(t => t.Id).ToList();

            var items = await _unitOfWork.Repository<ServiceTicketItem>()
                .GetAllWithIncludesAsync<ServiceTicketItem>(
                    i => ticketIds.Contains(i.ServiceTicketId) && i.PartId.HasValue,
                    i => i,
                    i => i.Part!);

            var result = items
                .GroupBy(i => new { i.PartId, Name = i.Part?.Name ?? "Unknown", SKU = i.Part?.SKU ?? "" })
                .Select(g => new PartUsageReportDto
                {
                    PartName = g.Key.Name,
                    SKU = g.Key.SKU,
                    TotalQuantity = g.Sum(i => i.Quantity),
                    TotalValue = g.Sum(i => i.Quantity * i.UnitPrice),
                    TimesUsed = g.Count()
                })
                .OrderByDescending(x => x.TotalQuantity)
                .ToList();

            return Result<List<PartUsageReportDto>>.Ok(result);
        }

        public async Task<string> ExportInvoicesCsvAsync(ReportFilterDto filter)
        {
            var dateTo = filter.DateTo.Date.AddDays(1);

            var invoices = await _unitOfWork.Repository<Invoice>()
                .GetAllWithIncludesAsync<Invoice>(
                    i => i.Status == InvoiceStatus.Paid
                      && i.CreatedAt >= filter.DateFrom.Date
                      && i.CreatedAt < dateTo,
                    i => i,
                    i => i.ServiceTicket);

            foreach (var inv in invoices)
            {
                if (inv.ServiceTicket != null && inv.ServiceTicket.Bike == null)
                    inv.ServiceTicket.Bike = await _unitOfWork.Repository<CustomerBike>().GetByIdAsync(inv.ServiceTicket.BikeId);
            }

            var sb = new StringBuilder();
            sb.AppendLine("Invoice #,Date,Bike,Subtotal,Tax,Discount,Total");

            foreach (var inv in invoices.OrderByDescending(i => i.CreatedAt))
            {
                var bike = inv.ServiceTicket?.Bike != null
                    ? $"{inv.ServiceTicket.Bike.Year} {inv.ServiceTicket.Bike.Make} {inv.ServiceTicket.Bike.Model}"
                    : "";
                sb.AppendLine($"{inv.Id},{inv.CreatedAt:yyyy-MM-dd},\"{bike}\",{inv.TotalAmount},{inv.TaxAmount},{inv.DiscountAmount},{inv.FinalAmount}");
            }

            return sb.ToString();
        }

        public async Task<string> ExportTicketsCsvAsync(ReportFilterDto filter)
        {
            var dateTo = filter.DateTo.Date.AddDays(1);

            var tickets = await _unitOfWork.Repository<ServiceTicket>()
                .GetAllWithIncludesAsync<ServiceTicket>(
                    t => t.CreatedAt >= filter.DateFrom.Date && t.CreatedAt < dateTo,
                    t => t,
                    t => t.Bike,
                    t => t.Mechanic!);

            var sb = new StringBuilder();
            sb.AppendLine("Ticket #,Created,Bike,Mechanic,Status,Est. Completion");

            foreach (var t in tickets.OrderByDescending(t => t.CreatedAt))
            {
                var bike = $"{t.Bike?.Year} {t.Bike?.Make} {t.Bike?.Model}".Trim();
                var mechanic = t.Mechanic?.FullName ?? "Unassigned";
                var est = t.EstimatedCompletionDate?.ToString("yyyy-MM-dd") ?? "";
                sb.AppendLine($"{t.Id},{t.CreatedAt:yyyy-MM-dd},\"{bike}\",\"{mechanic}\",{t.Status},{est}");
            }

            return sb.ToString();
        }

        public async Task<string> ExportPartUsageCsvAsync(ReportFilterDto filter)
        {
            var result = await GetPartUsageReportAsync(filter);
            var parts = result.Data ?? new();

            var sb = new StringBuilder();
            sb.AppendLine("Part Name,SKU,Times Used,Total Qty,Total Value");

            foreach (var p in parts)
                sb.AppendLine($"\"{p.PartName}\",{p.SKU},{p.TimesUsed},{p.TotalQuantity},{p.TotalValue}");

            return sb.ToString();
        }
    }
}
