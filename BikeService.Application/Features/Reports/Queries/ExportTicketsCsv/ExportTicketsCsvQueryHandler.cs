using System.Text;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Domain.Entities;
using MediatR;

namespace BikeService.Application.Features.Reports.Queries.ExportTicketsCsv;

public class ExportTicketsCsvQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<ExportTicketsCsvQuery, string>
{
    public async Task<string> Handle(ExportTicketsCsvQuery request, CancellationToken cancellationToken)
    {
        var filter = request.Filter;
        var dateTo = filter.DateTo.Date.AddDays(1);

        var tickets = await unitOfWork.Repository<ServiceTicket>()
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
}
