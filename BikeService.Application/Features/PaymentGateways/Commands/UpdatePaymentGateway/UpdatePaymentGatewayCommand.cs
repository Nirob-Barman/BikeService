using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.PaymentGateways.Commands.UpdatePaymentGateway;

public record UpdatePaymentGatewayCommand(
    int Id,
    string Slug,
    string Name,
    string Config,
    bool IsActive,
    bool IsSandbox) : IRequest<Result<bool>>;
