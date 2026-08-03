using System.Text;
using BikeService.Application.Features.Reports.Queries.GetPartUsageReport;
using BikeService.Application.Interfaces.Persistence;
using MediatR;

namespace BikeService.Application.Features.Reports.Queries.ExportPartUsageCsv;

public class ExportPartUsageCsvQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<ExportPartUsageCsvQuery, string>
{
    public async Task<string> Handle(ExportPartUsageCsvQuery request, CancellationToken cancellationToken)
    {
        var result = await new GetPartUsageReportQueryHandler(unitOfWork)
            .Handle(new GetPartUsageReportQuery(request.Filter), cancellationToken);
        var parts = result.Data ?? new();

        var sb = new StringBuilder();
        sb.AppendLine("Part Name,SKU,Times Used,Total Qty,Total Value");

        foreach (var p in parts)
            sb.AppendLine($"\"{p.PartName}\",{p.SKU},{p.TimesUsed},{p.TotalQuantity},{p.TotalValue}");

        return sb.ToString();
    }
}
