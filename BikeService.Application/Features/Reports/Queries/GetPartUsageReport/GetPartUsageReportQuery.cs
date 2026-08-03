using BikeService.Application.DTOs.Report;
using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.Reports.Queries.GetPartUsageReport;

public record GetPartUsageReportQuery(ReportFilterDto Filter) : IRequest<Result<List<PartUsageReportDto>>>;
