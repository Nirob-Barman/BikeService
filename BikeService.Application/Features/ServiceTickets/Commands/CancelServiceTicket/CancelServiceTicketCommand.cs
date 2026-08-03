using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.ServiceTickets.Commands.CancelServiceTicket;

public record CancelServiceTicketCommand(int Id) : IRequest<Result<bool>>;
