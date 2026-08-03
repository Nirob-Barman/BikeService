using BikeService.Application.DTOs.TicketNote;
using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.TicketNotes.Commands.AddTicketNote;

public record AddTicketNoteCommand(int ServiceTicketId, string Message) : IRequest<Result<TicketNoteDto>>;
