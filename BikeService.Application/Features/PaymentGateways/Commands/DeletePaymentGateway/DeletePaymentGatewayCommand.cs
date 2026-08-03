using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.PaymentGateways.Commands.DeletePaymentGateway;

public record DeletePaymentGatewayCommand(int Id) : IRequest<Result<bool>>;
