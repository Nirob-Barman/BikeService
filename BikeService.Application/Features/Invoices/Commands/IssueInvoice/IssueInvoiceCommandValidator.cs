using FluentValidation;

namespace BikeService.Application.Features.Invoices.Commands.IssueInvoice;

public class IssueInvoiceCommandValidator : AbstractValidator<IssueInvoiceCommand>
{
    public IssueInvoiceCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
