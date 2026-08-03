using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.ServiceTickets.Commands.UpdateServiceTicketDiagnosis;

public record UpdateServiceTicketDiagnosisCommand(int Id, string? Notes, DateTime? EstimatedCompletion) : IRequest<Result<bool>>;
