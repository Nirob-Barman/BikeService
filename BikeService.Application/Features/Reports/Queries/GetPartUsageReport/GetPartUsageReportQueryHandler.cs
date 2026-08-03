using BikeService.Application.DTOs.Report;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;
using BikeService.Domain.Enums;
using MediatR;

namespace BikeService.Application.Features.Reports.Queries.GetPartUsageReport;

public class GetPartUsageReportQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetPartUsageReportQuery, Result<List<PartUsageReportDto>>>
{
    public async Task<Result<List<PartUsageReportDto>>> Handle(GetPartUsageReportQuery request, CancellationToken cancellationToken)
    {
        var filter = request.Filter;
        var dateTo = filter.DateTo.Date.AddDays(1);

        // Get tickets created in range that reached InProgress or beyond
        var tickets = await unitOfWork.Repository<ServiceTicket>()
            .Where(t => t.CreatedAt >= filter.DateFrom.Date
                     && t.CreatedAt < dateTo
                     && t.Status != ServiceTicketStatus.Pending
                     && t.Status != ServiceTicketStatus.Diagnosed);

        var ticketIds = tickets.Select(t => t.Id).ToList();

        var items = await unitOfWork.Repository<ServiceTicketItem>()
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
}
