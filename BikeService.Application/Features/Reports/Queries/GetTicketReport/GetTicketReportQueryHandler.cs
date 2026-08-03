using BikeService.Application.DTOs.Report;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;
using BikeService.Domain.Enums;
using MediatR;

namespace BikeService.Application.Features.Reports.Queries.GetTicketReport;

public class GetTicketReportQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetTicketReportQuery, Result<TicketReportDto>>
{
    public async Task<Result<TicketReportDto>> Handle(GetTicketReportQuery request, CancellationToken cancellationToken)
    {
        var filter = request.Filter;
        var dateTo = filter.DateTo.Date.AddDays(1);

        var tickets = await unitOfWork.Repository<ServiceTicket>()
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
}
