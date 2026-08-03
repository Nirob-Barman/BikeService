using FluentValidation;

namespace BikeService.Application.Features.ServiceTickets.Commands.UpdateServiceTicketDiagnosis;

public class UpdateServiceTicketDiagnosisCommandValidator : AbstractValidator<UpdateServiceTicketDiagnosisCommand>
{
    public UpdateServiceTicketDiagnosisCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
