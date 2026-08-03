using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.ServiceTickets.Commands.AssignMechanicToTicket;

public record AssignMechanicToTicketCommand(int Id, int MechanicId) : IRequest<Result<bool>>;
