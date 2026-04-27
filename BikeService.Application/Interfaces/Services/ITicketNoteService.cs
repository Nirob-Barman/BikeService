using BikeService.Application.DTOs.TicketNote;
using BikeService.Application.Wrappers;

namespace BikeService.Application.Interfaces.Services
{
    public interface ITicketNoteService
    {
        Task<Result<List<TicketNoteDto>>> GetByTicketIdAsync(int ticketId);
        Task<Result<TicketNoteDto>> AddAsync(TicketNoteFormDto dto);
    }
}
