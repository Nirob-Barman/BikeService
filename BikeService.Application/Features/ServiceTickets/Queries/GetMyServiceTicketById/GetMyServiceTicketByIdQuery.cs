using BikeService.Application.DTOs.ServiceTicket;
using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.ServiceTickets.Queries.GetMyServiceTicketById;

public record GetMyServiceTicketByIdQuery(int Id) : IRequest<Result<ServiceTicketDto>>;
