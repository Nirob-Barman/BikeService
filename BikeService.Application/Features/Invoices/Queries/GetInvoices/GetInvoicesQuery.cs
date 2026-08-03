using BikeService.Application.DTOs.Invoice;
using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.Invoices.Queries.GetInvoices;

public record GetInvoicesQuery(InvoiceFilterDto? Filter = null) : IRequest<Result<List<InvoiceDto>>>;
