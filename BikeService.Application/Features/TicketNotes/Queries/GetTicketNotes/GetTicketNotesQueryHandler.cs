using BikeService.Application.DTOs.TicketNote;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Mappers;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;
using MediatR;

namespace BikeService.Application.Features.TicketNotes.Queries.GetTicketNotes;

public class GetTicketNotesQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetTicketNotesQuery, Result<List<TicketNoteDto>>>
{
    public async Task<Result<List<TicketNoteDto>>> Handle(GetTicketNotesQuery request, CancellationToken cancellationToken)
    {
        var notes = await unitOfWork.Repository<TicketNote>()
            .Where(n => n.ServiceTicketId == request.TicketId);

        var dtos = notes.OrderBy(n => n.CreatedAt)
                        .Select(TicketNoteMapper.ToDto)
                        .ToList();

        return Result<List<TicketNoteDto>>.Ok(dtos);
    }
}
