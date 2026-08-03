using BikeService.Application.Features.ServiceTickets.Commands.UpdateServiceTicketStatus;
using BikeService.Application.Wrappers;
using BikeService.Domain.Enums;
using MediatR;

namespace BikeService.Application.Features.ServiceTickets.Commands.CancelServiceTicket;

public class CancelServiceTicketCommandHandler(IMediator mediator) : IRequestHandler<CancelServiceTicketCommand, Result<bool>>
{
    public Task<Result<bool>> Handle(CancelServiceTicketCommand request, CancellationToken cancellationToken)
    {
        return mediator.Send(new UpdateServiceTicketStatusCommand(request.Id, ServiceTicketStatus.Cancelled), cancellationToken);
    }
}
