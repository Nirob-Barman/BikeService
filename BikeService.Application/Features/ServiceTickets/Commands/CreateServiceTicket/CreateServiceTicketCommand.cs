using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.ServiceTickets.Commands.CreateServiceTicket;

public record CreateServiceTicketCommand(
    int BikeId,
    int? MechanicId,
    int? AppointmentId,
    string? DiagnosisNotes,
    DateTime? EstimatedCompletionDate) : IRequest<Result<int>>;
