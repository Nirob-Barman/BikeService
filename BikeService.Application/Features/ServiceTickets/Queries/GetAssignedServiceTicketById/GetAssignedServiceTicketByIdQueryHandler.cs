using BikeService.Application.DTOs.ServiceTicket;
using BikeService.Application.Features.ServiceTickets.Queries.GetServiceTicketById;
using BikeService.Application.Interfaces;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;
using MediatR;

namespace BikeService.Application.Features.ServiceTickets.Queries.GetAssignedServiceTicketById;

public class GetAssignedServiceTicketByIdQueryHandler(
    IUnitOfWork unitOfWork,
    IUserContextService userContextService,
    IMediator mediator) : IRequestHandler<GetAssignedServiceTicketByIdQuery, Result<ServiceTicketDto>>
{
    public async Task<Result<ServiceTicketDto>> Handle(GetAssignedServiceTicketByIdQuery request, CancellationToken cancellationToken)
    {
        var userId = userContextService.UserId;
        if (string.IsNullOrEmpty(userId))
            return Result<ServiceTicketDto>.Fail("User not authenticated.");

        var mechanic = await unitOfWork.Repository<Mechanic>()
            .FirstOrDefaultAsync(m => m.UserId == userId);

        if (mechanic == null)
            return Result<ServiceTicketDto>.Fail("Mechanic profile not found for this account.");

        var ticketResult = await mediator.Send(new GetServiceTicketByIdQuery(request.Id), cancellationToken);
        if (!ticketResult.Success)
            return ticketResult;

        if (ticketResult.Data!.MechanicId != mechanic.Id)
            return Result<ServiceTicketDto>.Fail("Access denied.");

        return ticketResult;
    }
}
