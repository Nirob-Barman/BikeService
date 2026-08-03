using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.Payments.Commands.InitiatePayment;

public record InitiatePaymentCommand(int InvoiceId, int GatewayId, string? PromoCode) : IRequest<Result<string>>;
