using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.ServiceTickets.Commands.RemoveServiceTicketItem;

public record RemoveServiceTicketItemCommand(int ItemId) : IRequest<Result<bool>>;
