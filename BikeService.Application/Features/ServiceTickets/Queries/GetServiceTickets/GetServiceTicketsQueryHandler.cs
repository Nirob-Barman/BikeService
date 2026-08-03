using BikeService.Application.DTOs.ServiceTicket;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Mappers;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;
using MediatR;

namespace BikeService.Application.Features.ServiceTickets.Queries.GetServiceTickets;

public class GetServiceTicketsQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetServiceTicketsQuery, Result<List<ServiceTicketDto>>>
{
    public async Task<Result<List<ServiceTicketDto>>> Handle(GetServiceTicketsQuery request, CancellationToken cancellationToken)
    {
        var tickets = await unitOfWork.Repository<ServiceTicket>()
            .GetAllWithIncludesAsync<ServiceTicket>(
                t => t,
                t => t.Bike,
                t => t.Mechanic,
                t => t.Items,
                t => t.Invoices);

        var filter = request.Filter;
        if (filter != null)
        {
            if (filter.Status.HasValue)
                tickets = tickets.Where(t => t.Status == filter.Status.Value);
            if (filter.MechanicId.HasValue)
                tickets = tickets.Where(t => t.MechanicId == filter.MechanicId.Value);
            if (filter.DateFrom.HasValue)
                tickets = tickets.Where(t => t.CreatedAt >= filter.DateFrom.Value);
            if (filter.DateTo.HasValue)
                tickets = tickets.Where(t => t.CreatedAt <= filter.DateTo.Value);
            if (!string.IsNullOrEmpty(filter.CustomerId))
                tickets = tickets.Where(t => t.Bike != null && t.Bike.CustomerId == filter.CustomerId);
        }

        var dtos = tickets.Select(ServiceTicketMapper.ToDto).ToList();
        return Result<List<ServiceTicketDto>>.Ok(dtos);
    }
}
