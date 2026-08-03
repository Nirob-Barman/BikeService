using FluentValidation;

namespace BikeService.Application.Features.TicketNotes.Commands.AddTicketNote;

public class AddTicketNoteCommandValidator : AbstractValidator<AddTicketNoteCommand>
{
    public AddTicketNoteCommandValidator()
    {
        RuleFor(x => x.Message).NotEmpty().MaximumLength(1000);
    }
}
