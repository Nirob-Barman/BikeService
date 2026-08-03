using BikeService.Application.DTOs.Invoice;
using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.Invoices.Queries.GetMyInvoices;

public record GetMyInvoicesQuery : IRequest<Result<List<InvoiceDto>>>;
