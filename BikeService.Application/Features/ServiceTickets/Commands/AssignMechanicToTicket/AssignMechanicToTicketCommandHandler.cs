using System.Text.Json;
using BikeService.Application.Interfaces;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;
using MediatR;

namespace BikeService.Application.Features.ServiceTickets.Commands.AssignMechanicToTicket;

public class AssignMechanicToTicketCommandHandler(
    IUnitOfWork unitOfWork,
    IAuditLogService auditLogService,
    IUserContextService userContextService) : IRequestHandler<AssignMechanicToTicketCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(AssignMechanicToTicketCommand request, CancellationToken cancellationToken)
    {
        var ticket = await unitOfWork.Repository<ServiceTicket>().GetByIdAsync(request.Id);
        if (ticket == null)
            return Result<bool>.Fail("Service ticket not found.");

        var mechanic = await unitOfWork.Repository<Mechanic>().GetByIdAsync(request.MechanicId);
        if (mechanic == null)
            return Result<bool>.Fail("Mechanic not found.");

        var oldValues = JsonSerializer.Serialize(new { ticket.MechanicId });

        ticket.MechanicId = request.MechanicId;
        ticket.UpdatedBy = userContextService.UserId;
        ticket.UpdatedAt = DateTime.UtcNow;

        unitOfWork.Repository<ServiceTicket>().Update(ticket);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await auditLogService.LogAsync(
            "ServiceTicket", "AssignMechanic",
            userContextService.UserId, userContextService.Email,
            $"Assigned mechanic '{mechanic.FullName}' to ticket ID {request.Id}",
            entityId: request.Id.ToString(),
            ipAddress: userContextService.IpAddress,
            userAgent: userContextService.UserAgent,
            oldValues: oldValues,
            newValues: JsonSerializer.Serialize(new { MechanicId = request.MechanicId }));

        return Result<bool>.Ok(true, "Mechanic assigned successfully.");
    }
}
