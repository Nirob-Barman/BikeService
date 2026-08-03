using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.PaymentGateways.Commands.CreatePaymentGateway;

public record CreatePaymentGatewayCommand(
    string Slug,
    string Name,
    string Config,
    bool IsActive,
    bool IsSandbox) : IRequest<Result<int>>;
