using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.Payments.Commands.HandlePaymentSuccess;

public record HandlePaymentSuccessCommand(int TxId, Dictionary<string, string> CallbackParams) : IRequest<Result<bool>>;
