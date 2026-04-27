using BikeService.Application.DTOs.TicketNote;
using BikeService.Application.Interfaces;
using BikeService.Application.Interfaces.Identity;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Interfaces.Services;
using BikeService.Application.Mappers;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;

namespace BikeService.Application.Services
{
    public class TicketNoteService : ITicketNoteService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserContextService _userContextService;
        private readonly IUserManager _userManager;
        private readonly INotificationService _notificationService;

        public TicketNoteService(
            IUnitOfWork unitOfWork,
            IUserContextService userContextService,
            IUserManager userManager,
            INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _userContextService = userContextService;
            _userManager = userManager;
            _notificationService = notificationService;
        }

        public async Task<Result<List<TicketNoteDto>>> GetByTicketIdAsync(int ticketId)
        {
            var notes = await _unitOfWork.Repository<TicketNote>()
                .Where(n => n.ServiceTicketId == ticketId);

            var dtos = notes.OrderBy(n => n.CreatedAt)
                            .Select(TicketNoteMapper.ToDto)
                            .ToList();

            return Result<List<TicketNoteDto>>.Ok(dtos);
        }

        public async Task<Result<TicketNoteDto>> AddAsync(TicketNoteFormDto dto)
        {
            var userId = _userContextService.UserId;
            if (string.IsNullOrEmpty(userId))
                return Result<TicketNoteDto>.Fail("User is not authenticated.");

            if (string.IsNullOrWhiteSpace(dto.Message))
                return Result<TicketNoteDto>.FailField("Message", "Message cannot be empty.");

            if (dto.Message.Length > 1000)
                return Result<TicketNoteDto>.FailField("Message", "Message cannot exceed 1000 characters.");

            var ticket = await _unitOfWork.Repository<ServiceTicket>()
                .GetByIdAsync(dto.ServiceTicketId);
            if (ticket == null)
                return Result<TicketNoteDto>.Fail("Service ticket not found.");

            // Determine role and verify access
            var authorRole = string.Empty;

            if (_userContextService.IsInRole("Customer"))
            {
                var bike = await _unitOfWork.Repository<CustomerBike>().GetByIdAsync(ticket.BikeId);
                if (bike?.CustomerId != userId)
                    return Result<TicketNoteDto>.Fail("Access denied.");
                authorRole = "Customer";
            }
            else if (_userContextService.IsInRole("Mechanic"))
            {
                var mechanic = await _unitOfWork.Repository<Mechanic>()
                    .FirstOrDefaultAsync(m => m.UserId == userId);
                if (mechanic == null || ticket.MechanicId != mechanic.Id)
                    return Result<TicketNoteDto>.Fail("Access denied.");
                authorRole = "Mechanic";
            }
            else
            {
                return Result<TicketNoteDto>.Fail("Access denied.");
            }

            // Resolve display name
            var user = await _userManager.FindByIdAsync(userId);
            var authorName = user?.FullName ?? "Unknown";

            var note = new TicketNote
            {
                ServiceTicketId = dto.ServiceTicketId,
                AuthorId = userId,
                AuthorName = authorName,
                AuthorRole = authorRole,
                Message = dto.Message.Trim(),
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            };

            await _unitOfWork.Repository<TicketNote>().AddAsync(note);
            await _unitOfWork.SaveChangesAsync();

            // Notify the other party
            await NotifyOtherPartyAsync(ticket, note, authorRole);

            return Result<TicketNoteDto>.Ok(TicketNoteMapper.ToDto(note), "Note added.");
        }

        private async Task NotifyOtherPartyAsync(ServiceTicket ticket, TicketNote note, string authorRole)
        {
            if (authorRole == "Customer")
            {
                // Notify the assigned mechanic (if any)
                if (ticket.MechanicId.HasValue)
                {
                    var mechanic = await _unitOfWork.Repository<Mechanic>().GetByIdAsync(ticket.MechanicId.Value);
                    if (mechanic != null && !string.IsNullOrEmpty(mechanic.UserId))
                    {
                        await _notificationService.CreateNotificationAsync(
                            mechanic.UserId,
                            "New Customer Note",
                            $"{note.AuthorName} left a note on ticket #{ticket.Id}.",
                            link: $"/Mechanic/Detail/{ticket.Id}");
                    }
                }
            }
            else if (authorRole == "Mechanic")
            {
                // Notify the customer who owns the bike
                var bike = await _unitOfWork.Repository<CustomerBike>().GetByIdAsync(ticket.BikeId);
                if (bike != null && !string.IsNullOrEmpty(bike.CustomerId))
                {
                    await _notificationService.CreateNotificationAsync(
                        bike.CustomerId,
                        "New Mechanic Note",
                        $"{note.AuthorName} replied on ticket #{ticket.Id}.",
                        link: $"/ServiceTicket/Detail/{ticket.Id}");
                }
            }
        }
    }
}
