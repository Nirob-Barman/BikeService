using BikeService.Application.DTOs.Invoice;
using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.Invoices.Queries.GetMyInvoiceById;

public record GetMyInvoiceByIdQuery(int Id) : IRequest<Result<InvoiceDto>>;
