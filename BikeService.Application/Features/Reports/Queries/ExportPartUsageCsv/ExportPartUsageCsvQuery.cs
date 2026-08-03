using BikeService.Application.DTOs.Report;
using MediatR;

namespace BikeService.Application.Features.Reports.Queries.ExportPartUsageCsv;

public record ExportPartUsageCsvQuery(ReportFilterDto Filter) : IRequest<string>;
