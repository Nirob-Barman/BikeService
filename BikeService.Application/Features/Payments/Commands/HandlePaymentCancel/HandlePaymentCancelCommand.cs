using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.Payments.Commands.HandlePaymentCancel;

public record HandlePaymentCancelCommand(int TxId) : IRequest<Result<bool>>;
