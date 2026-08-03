using BikeService.Application.DTOs.PaymentGateway;
using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.PaymentGateways.Queries.GetPaymentGateways;

public record GetPaymentGatewaysQuery : IRequest<Result<List<PaymentGatewayDto>>>;
