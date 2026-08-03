using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.Invoices.Commands.IssueInvoice;

public record IssueInvoiceCommand(int Id) : IRequest<Result<bool>>;
