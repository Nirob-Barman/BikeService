using FluentValidation;

namespace BikeService.Application.Features.Invoices.Commands.VoidInvoice;

public class VoidInvoiceCommandValidator : AbstractValidator<VoidInvoiceCommand>
{
    public VoidInvoiceCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
