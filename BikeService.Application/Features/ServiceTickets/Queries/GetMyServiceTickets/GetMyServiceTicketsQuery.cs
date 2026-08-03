using BikeService.Application.DTOs.ServiceTicket;
using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.ServiceTickets.Queries.GetMyServiceTickets;

public record GetMyServiceTicketsQuery : IRequest<Result<List<ServiceTicketDto>>>;
