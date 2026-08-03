using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.Invoices.Commands.VoidInvoice;

public record VoidInvoiceCommand(int Id) : IRequest<Result<bool>>;
