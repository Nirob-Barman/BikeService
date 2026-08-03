using BikeService.Application.DTOs.ServiceTicket;
using BikeService.Application.Features.ServiceTickets.Queries.GetServiceTickets;
using BikeService.Application.Interfaces;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;
using MediatR;

namespace BikeService.Application.Features.ServiceTickets.Queries.GetAssignedServiceTickets;

public class GetAssignedServiceTicketsQueryHandler(
    IUnitOfWork unitOfWork,
    IUserContextService userContextService,
    IMediator mediator) : IRequestHandler<GetAssignedServiceTicketsQuery, Result<List<ServiceTicketDto>>>
{
    public async Task<Result<List<ServiceTicketDto>>> Handle(GetAssignedServiceTicketsQuery request, CancellationToken cancellationToken)
    {
        var userId = userContextService.UserId;
        if (string.IsNullOrEmpty(userId))
            return Result<List<ServiceTicketDto>>.Fail("User not authenticated.");

        var mechanic = await unitOfWork.Repository<Mechanic>()
            .FirstOrDefaultAsync(m => m.UserId == userId);

        if (mechanic == null)
            return Result<List<ServiceTicketDto>>.Fail("Mechanic profile not found for this account.");

        return await mediator.Send(
            new GetServiceTicketsQuery(new DTOs.ServiceTicket.TicketFilterDto { MechanicId = mechanic.Id }),
            cancellationToken);
    }
}
