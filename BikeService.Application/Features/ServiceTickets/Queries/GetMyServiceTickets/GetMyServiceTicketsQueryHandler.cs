using BikeService.Application.DTOs.ServiceTicket;
using BikeService.Application.Interfaces;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Mappers;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;
using MediatR;

namespace BikeService.Application.Features.ServiceTickets.Queries.GetMyServiceTickets;

public class GetMyServiceTicketsQueryHandler(IUnitOfWork unitOfWork, IUserContextService userContextService)
    : IRequestHandler<GetMyServiceTicketsQuery, Result<List<ServiceTicketDto>>>
{
    public async Task<Result<List<ServiceTicketDto>>> Handle(GetMyServiceTicketsQuery request, CancellationToken cancellationToken)
    {
        var customerId = userContextService.UserId;
        if (string.IsNullOrEmpty(customerId))
            return Result<List<ServiceTicketDto>>.Fail("User not authenticated.");

        var tickets = await unitOfWork.Repository<ServiceTicket>()
            .GetAllWithIncludesAsync<ServiceTicket>(
                t => t.Bike != null && t.Bike.CustomerId == customerId,
                t => t,
                t => t.Bike,
                t => t.Mechanic,
                t => t.Items,
                t => t.Invoices);

        var dtos = tickets.Select(ServiceTicketMapper.ToDto).ToList();
        return Result<List<ServiceTicketDto>>.Ok(dtos);
    }
}
