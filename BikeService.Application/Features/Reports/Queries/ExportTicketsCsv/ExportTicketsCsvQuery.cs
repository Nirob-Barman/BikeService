using BikeService.Application.DTOs.Report;
using MediatR;

namespace BikeService.Application.Features.Reports.Queries.ExportTicketsCsv;

public record ExportTicketsCsvQuery(ReportFilterDto Filter) : IRequest<string>;
