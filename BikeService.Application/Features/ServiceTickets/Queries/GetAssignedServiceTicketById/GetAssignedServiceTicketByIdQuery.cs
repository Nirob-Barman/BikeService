using BikeService.Application.DTOs.ServiceTicket;
using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.ServiceTickets.Queries.GetAssignedServiceTicketById;

public record GetAssignedServiceTicketByIdQuery(int Id) : IRequest<Result<ServiceTicketDto>>;
