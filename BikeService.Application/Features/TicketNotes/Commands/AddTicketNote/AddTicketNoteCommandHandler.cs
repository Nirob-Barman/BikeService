using BikeService.Application.DTOs.TicketNote;
using BikeService.Application.Interfaces;
using BikeService.Application.Interfaces.Identity;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Mappers;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;
using MediatR;

namespace BikeService.Application.Features.TicketNotes.Commands.AddTicketNote;

public class AddTicketNoteCommandHandler(
    IUnitOfWork unitOfWork,
    IUserContextService userContextService,
    IUserManager userManager,
    INotificationService notificationService) : IRequestHandler<AddTicketNoteCommand, Result<TicketNoteDto>>
{
    public async Task<Result<TicketNoteDto>> Handle(AddTicketNoteCommand request, CancellationToken cancellationToken)
    {
        var userId = userContextService.UserId;
        if (string.IsNullOrEmpty(userId))
            return Result<TicketNoteDto>.Fail("User is not authenticated.");

        if (string.IsNullOrWhiteSpace(request.Message))
            return Result<TicketNoteDto>.FailField("Message", "Message cannot be empty.");

        if (request.Message.Length > 1000)
            return Result<TicketNoteDto>.FailField("Message", "Message cannot exceed 1000 characters.");

        var ticket = await unitOfWork.Repository<ServiceTicket>()
            .GetByIdAsync(request.ServiceTicketId);
        if (ticket == null)
            return Result<TicketNoteDto>.Fail("Service ticket not found.");

        var authorRole = string.Empty;

        if (userContextService.IsInRole("Customer"))
        {
            var bike = await unitOfWork.Repository<CustomerBike>().GetByIdAsync(ticket.BikeId);
            if (bike?.CustomerId != userId)
                return Result<TicketNoteDto>.Fail("Access denied.");
            authorRole = "Customer";
        }
        else if (userContextService.IsInRole("Mechanic"))
        {
            var mechanic = await unitOfWork.Repository<Mechanic>()
                .FirstOrDefaultAsync(m => m.UserId == userId);
            if (mechanic == null || ticket.MechanicId != mechanic.Id)
                return Result<TicketNoteDto>.Fail("Access denied.");
            authorRole = "Mechanic";
        }
        else
        {
            return Result<TicketNoteDto>.Fail("Access denied.");
        }

        var user = await userManager.FindByIdAsync(userId);
        var authorName = user?.FullName ?? "Unknown";

        var note = new TicketNote
        {
            ServiceTicketId = request.ServiceTicketId,
            AuthorId = userId,
            AuthorName = authorName,
            AuthorRole = authorRole,
            Message = request.Message.Trim(),
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };

        await unitOfWork.Repository<TicketNote>().AddAsync(note);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await NotifyOtherPartyAsync(ticket, note, authorRole);

        return Result<TicketNoteDto>.Ok(TicketNoteMapper.ToDto(note), "Note added.");
    }

    private async Task NotifyOtherPartyAsync(ServiceTicket ticket, TicketNote note, string authorRole)
    {
        if (authorRole == "Customer")
        {
            if (ticket.MechanicId.HasValue)
            {
                var mechanic = await unitOfWork.Repository<Mechanic>().GetByIdAsync(ticket.MechanicId.Value);
                if (mechanic != null && !string.IsNullOrEmpty(mechanic.UserId))
                {
                    await notificationService.CreateNotificationAsync(
                        mechanic.UserId,
                        "New Customer Note",
                        $"{note.AuthorName} left a note on ticket #{ticket.Id}.",
                        link: $"/Mechanic/Detail/{ticket.Id}");
                }
            }
        }
        else if (authorRole == "Mechanic")
        {
            var bike = await unitOfWork.Repository<CustomerBike>().GetByIdAsync(ticket.BikeId);
            if (bike != null && !string.IsNullOrEmpty(bike.CustomerId))
            {
                await notificationService.CreateNotificationAsync(
                    bike.CustomerId,
                    "New Mechanic Note",
                    $"{note.AuthorName} replied on ticket #{ticket.Id}.",
                    link: $"/ServiceTicket/Detail/{ticket.Id}");
            }
        }
    }
}
