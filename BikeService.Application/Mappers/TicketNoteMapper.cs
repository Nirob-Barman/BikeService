using BikeService.Application.DTOs.TicketNote;
using BikeService.Domain.Entities;

namespace BikeService.Application.Mappers
{
    public static class TicketNoteMapper
    {
        public static TicketNoteDto ToDto(TicketNote n) => new()
        {
            Id = n.Id,
            ServiceTicketId = n.ServiceTicketId,
            AuthorId = n.AuthorId,
            AuthorName = n.AuthorName,
            AuthorRole = n.AuthorRole,
            Message = n.Message,
            CreatedAt = n.CreatedAt
        };
    }
}
