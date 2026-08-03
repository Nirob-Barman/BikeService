using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.PaymentGateways.Queries.GetDecryptedPaymentGatewayConfig;

public record GetDecryptedPaymentGatewayConfigQuery(int Id) : IRequest<Result<string>>;
