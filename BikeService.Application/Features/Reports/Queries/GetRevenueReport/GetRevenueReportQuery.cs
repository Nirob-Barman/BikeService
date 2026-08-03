using BikeService.Application.DTOs.Report;
using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.Reports.Queries.GetRevenueReport;

public record GetRevenueReportQuery(ReportFilterDto Filter) : IRequest<Result<RevenueReportDto>>;
