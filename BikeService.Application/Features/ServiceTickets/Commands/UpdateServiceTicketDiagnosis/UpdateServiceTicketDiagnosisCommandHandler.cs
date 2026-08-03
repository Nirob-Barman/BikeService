using System.Text.Json;
using BikeService.Application.Interfaces;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;
using MediatR;

namespace BikeService.Application.Features.ServiceTickets.Commands.UpdateServiceTicketDiagnosis;

public class UpdateServiceTicketDiagnosisCommandHandler(
    IUnitOfWork unitOfWork,
    IAuditLogService auditLogService,
    IUserContextService userContextService) : IRequestHandler<UpdateServiceTicketDiagnosisCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(UpdateServiceTicketDiagnosisCommand request, CancellationToken cancellationToken)
    {
        var ticket = await unitOfWork.Repository<ServiceTicket>().GetByIdAsync(request.Id);
        if (ticket == null)
            return Result<bool>.Fail("Service ticket not found.");

        var oldValues = JsonSerializer.Serialize(new
        {
            ticket.DiagnosisNotes,
            ticket.EstimatedCompletionDate
        });

        ticket.DiagnosisNotes = request.Notes;
        ticket.EstimatedCompletionDate = request.EstimatedCompletion;
        ticket.UpdatedBy = userContextService.UserId;
        ticket.UpdatedAt = DateTime.UtcNow;

        unitOfWork.Repository<ServiceTicket>().Update(ticket);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await auditLogService.LogAsync(
            "ServiceTicket", "UpdateDiagnosis",
            userContextService.UserId, userContextService.Email,
            $"Updated diagnosis notes and estimated completion for ticket ID {request.Id}",
            entityId: request.Id.ToString(),
            ipAddress: userContextService.IpAddress,
            userAgent: userContextService.UserAgent,
            oldValues: oldValues,
            newValues: JsonSerializer.Serialize(new
            {
                DiagnosisNotes = request.Notes,
                EstimatedCompletionDate = request.EstimatedCompletion
            }));

        return Result<bool>.Ok(true, "Diagnosis updated successfully.");
    }
}
