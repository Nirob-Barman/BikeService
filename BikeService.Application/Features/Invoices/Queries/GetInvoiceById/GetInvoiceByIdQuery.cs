using BikeService.Application.DTOs.Invoice;
using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.Invoices.Queries.GetInvoiceById;

public record GetInvoiceByIdQuery(int Id) : IRequest<Result<InvoiceDto>>;
