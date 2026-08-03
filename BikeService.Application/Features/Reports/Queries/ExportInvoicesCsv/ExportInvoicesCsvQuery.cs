using BikeService.Application.DTOs.Report;
using MediatR;

namespace BikeService.Application.Features.Reports.Queries.ExportInvoicesCsv;

public record ExportInvoicesCsvQuery(ReportFilterDto Filter) : IRequest<string>;
