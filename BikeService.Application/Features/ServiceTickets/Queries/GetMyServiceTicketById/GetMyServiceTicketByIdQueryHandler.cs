using BikeService.Application.DTOs.ServiceTicket;
using BikeService.Application.Features.ServiceTickets.Queries.GetServiceTicketById;
using BikeService.Application.Interfaces;
using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.ServiceTickets.Queries.GetMyServiceTicketById;

public class GetMyServiceTicketByIdQueryHandler(
    IUserContextService userContextService,
    IMediator mediator) : IRequestHandler<GetMyServiceTicketByIdQuery, Result<ServiceTicketDto>>
{
    public async Task<Result<ServiceTicketDto>> Handle(GetMyServiceTicketByIdQuery request, CancellationToken cancellationToken)
    {
        var userId = userContextService.UserId;
        if (string.IsNullOrEmpty(userId))
            return Result<ServiceTicketDto>.Fail("User not authenticated.");

        var ticketResult = await mediator.Send(new GetServiceTicketByIdQuery(request.Id), cancellationToken);
        if (!ticketResult.Success)
            return ticketResult;

        if (ticketResult.Data!.CustomerId != userId)
            return Result<ServiceTicketDto>.Fail("Access denied.");

        return ticketResult;
    }
}
