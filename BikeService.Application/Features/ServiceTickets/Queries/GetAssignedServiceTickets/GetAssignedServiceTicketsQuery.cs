using BikeService.Application.DTOs.ServiceTicket;
using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.ServiceTickets.Queries.GetAssignedServiceTickets;

public record GetAssignedServiceTicketsQuery : IRequest<Result<List<ServiceTicketDto>>>;
