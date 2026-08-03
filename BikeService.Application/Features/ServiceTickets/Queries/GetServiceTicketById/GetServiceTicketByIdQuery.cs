using BikeService.Application.DTOs.ServiceTicket;
using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.ServiceTickets.Queries.GetServiceTicketById;

public record GetServiceTicketByIdQuery(int Id) : IRequest<Result<ServiceTicketDto>>;
