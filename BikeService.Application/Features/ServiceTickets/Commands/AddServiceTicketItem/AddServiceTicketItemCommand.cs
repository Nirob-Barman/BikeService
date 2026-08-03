using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.ServiceTickets.Commands.AddServiceTicketItem;

public record AddServiceTicketItemCommand(
    int TicketId,
    int? ServiceTypeId,
    int? PartId,
    int Quantity,
    decimal UnitPrice) : IRequest<Result<bool>>;
