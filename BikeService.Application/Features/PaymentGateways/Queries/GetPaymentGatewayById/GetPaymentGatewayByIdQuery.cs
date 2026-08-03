using BikeService.Application.DTOs.PaymentGateway;
using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.PaymentGateways.Queries.GetPaymentGatewayById;

public record GetPaymentGatewayByIdQuery(int Id) : IRequest<Result<PaymentGatewayDto>>;
