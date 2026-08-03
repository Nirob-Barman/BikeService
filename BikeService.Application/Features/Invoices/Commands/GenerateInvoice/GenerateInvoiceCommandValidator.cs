using FluentValidation;

namespace BikeService.Application.Features.Invoices.Commands.GenerateInvoice;

public class GenerateInvoiceCommandValidator : AbstractValidator<GenerateInvoiceCommand>
{
    public GenerateInvoiceCommandValidator()
    {
        RuleFor(x => x.TicketId).GreaterThan(0);
    }
}
