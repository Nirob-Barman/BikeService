using BikeService.Application.Wrappers;
using BikeService.Domain.Enums;
using MediatR;

namespace BikeService.Application.Features.ServiceTickets.Commands.UpdateServiceTicketStatus;

public record UpdateServiceTicketStatusCommand(int Id, ServiceTicketStatus NewStatus) : IRequest<Result<bool>>;
