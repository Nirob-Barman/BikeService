using BikeService.Application.DTOs.Report;
using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.Reports.Queries.GetTicketReport;

public record GetTicketReportQuery(ReportFilterDto Filter) : IRequest<Result<TicketReportDto>>;
