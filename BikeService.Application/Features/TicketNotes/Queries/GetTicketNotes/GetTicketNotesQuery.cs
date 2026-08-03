using BikeService.Application.DTOs.TicketNote;
using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.TicketNotes.Queries.GetTicketNotes;

public record GetTicketNotesQuery(int TicketId) : IRequest<Result<List<TicketNoteDto>>>;
