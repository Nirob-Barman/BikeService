using BikeService.Application.DTOs.ServiceTicket;
using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.ServiceTickets.Queries.GetServiceTickets;

public record GetServiceTicketsQuery(TicketFilterDto? Filter = null) : IRequest<Result<List<ServiceTicketDto>>>;
