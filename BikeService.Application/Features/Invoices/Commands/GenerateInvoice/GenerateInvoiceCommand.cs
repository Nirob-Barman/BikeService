using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.Invoices.Commands.GenerateInvoice;

public record GenerateInvoiceCommand(int TicketId) : IRequest<Result<int>>;
