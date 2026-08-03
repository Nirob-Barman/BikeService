using BikeService.Application.DTOs.Report;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;
using BikeService.Domain.Enums;
using MediatR;

namespace BikeService.Application.Features.Reports.Queries.GetRevenueReport;

public class GetRevenueReportQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetRevenueReportQuery, Result<RevenueReportDto>>
{
    public async Task<Result<RevenueReportDto>> Handle(GetRevenueReportQuery request, CancellationToken cancellationToken)
    {
        var filter = request.Filter;
        var dateTo = filter.DateTo.Date.AddDays(1); // inclusive

        var invoices = await unitOfWork.Repository<Invoice>()
            .Where(i => i.Status == InvoiceStatus.Paid
                     && i.CreatedAt >= filter.DateFrom.Date
                     && i.CreatedAt < dateTo);

        var invoiceList = invoices.ToList();

        // Revenue by service type
        var ticketIds = invoiceList.Select(i => i.ServiceTicketId).ToList();

        var items = await unitOfWork.Repository<ServiceTicketItem>()
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
        var tickets = await unitOfWork.Repository<ServiceTicket>()
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
}
