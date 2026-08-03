using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.PaymentGateways.Commands.TogglePaymentGatewayActive;

public record TogglePaymentGatewayActiveCommand(int Id) : IRequest<Result<bool>>;
