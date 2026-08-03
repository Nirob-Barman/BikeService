using System.Text.Json;
using BikeService.Application.Interfaces;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;
using MediatR;

namespace BikeService.Application.Features.ServiceTickets.Commands.CreateServiceTicket;

public class CreateServiceTicketCommandHandler(
    IUnitOfWork unitOfWork,
    IAuditLogService auditLogService,
    IUserContextService userContextService) : IRequestHandler<CreateServiceTicketCommand, Result<int>>
{
    public async Task<Result<int>> Handle(CreateServiceTicketCommand request, CancellationToken cancellationToken)
    {
        var bike = await unitOfWork.Repository<CustomerBike>().GetByIdAsync(request.BikeId);
        if (bike == null)
            return Result<int>.Fail("Bike not found.");

        if (request.AppointmentId.HasValue)
        {
            var exists = await unitOfWork.Repository<ServiceTicket>()
                .AnyAsync(t => t.AppointmentId == request.AppointmentId.Value);
            if (exists)
                return Result<int>.Fail("A service ticket already exists for this appointment.");
        }

        if (request.MechanicId.HasValue)
        {
            var mechanic = await unitOfWork.Repository<Mechanic>().GetByIdAsync(request.MechanicId.Value);
            if (mechanic == null)
                return Result<int>.Fail("Mechanic not found.");
        }

        var ticket = new ServiceTicket
        {
            BikeId = request.BikeId,
            MechanicId = request.MechanicId,
            AppointmentId = request.AppointmentId,
            DiagnosisNotes = request.DiagnosisNotes,
            EstimatedCompletionDate = request.EstimatedCompletionDate,
            CreatedBy = userContextService.UserId,
            CreatedAt = DateTime.UtcNow
        };

        await unitOfWork.Repository<ServiceTicket>().AddAsync(ticket);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await auditLogService.LogAsync(
            "ServiceTicket", "Create",
            userContextService.UserId, userContextService.Email,
            $"Created service ticket for bike ID {request.BikeId}",
            entityId: ticket.Id.ToString(),
            ipAddress: userContextService.IpAddress,
            userAgent: userContextService.UserAgent,
            oldValues: null,
            newValues: JsonSerializer.Serialize(new
            {
                ticket.BikeId,
                ticket.MechanicId,
                ticket.AppointmentId,
                ticket.Status,
                ticket.DiagnosisNotes,
                ticket.EstimatedCompletionDate
            }));

        return Result<int>.Ok(ticket.Id, "Service ticket created successfully.");
    }
}
