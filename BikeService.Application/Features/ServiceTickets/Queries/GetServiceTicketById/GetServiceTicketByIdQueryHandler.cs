using BikeService.Application.DTOs.ServiceTicket;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Mappers;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;
using MediatR;

namespace BikeService.Application.Features.ServiceTickets.Queries.GetServiceTicketById;

public class GetServiceTicketByIdQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetServiceTicketByIdQuery, Result<ServiceTicketDto>>
{
    public async Task<Result<ServiceTicketDto>> Handle(GetServiceTicketByIdQuery request, CancellationToken cancellationToken)
    {
        var tickets = await unitOfWork.Repository<ServiceTicket>()
            .GetAllWithIncludesAsync<ServiceTicket>(
                t => t.Id == request.Id,
                t => t,
                t => t.Bike,
                t => t.Mechanic,
                t => t.Items,
                t => t.Invoices);

        var ticket = tickets.FirstOrDefault();
        if (ticket == null)
            return Result<ServiceTicketDto>.Fail("Service ticket not found.");

        foreach (var item in ticket.Items)
        {
            if (item.ServiceTypeId.HasValue && item.ServiceType == null)
            {
                item.ServiceType = await unitOfWork.Repository<ServiceType>().GetByIdAsync(item.ServiceTypeId.Value);
            }
            if (item.PartId.HasValue && item.Part == null)
            {
                item.Part = await unitOfWork.Repository<Part>().GetByIdAsync(item.PartId.Value);
            }
        }

        return Result<ServiceTicketDto>.Ok(ServiceTicketMapper.ToDto(ticket));
    }
}
