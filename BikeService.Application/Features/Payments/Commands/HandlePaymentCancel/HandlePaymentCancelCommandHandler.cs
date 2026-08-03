using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;
using BikeService.Domain.Enums;
using MediatR;

namespace BikeService.Application.Features.Payments.Commands.HandlePaymentCancel;

public class HandlePaymentCancelCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<HandlePaymentCancelCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(HandlePaymentCancelCommand request, CancellationToken cancellationToken)
    {
        var tx = await unitOfWork.Repository<PaymentTransaction>().GetByIdAsync(request.TxId);
        if (tx == null)
            return Result<bool>.Fail("Transaction not found.");

        if (tx.Status == PaymentTransactionStatus.Pending)
        {
            tx.Status = PaymentTransactionStatus.Failed;
            tx.UpdatedAt = DateTime.UtcNow;
            unitOfWork.Repository<PaymentTransaction>().Update(tx);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return Result<bool>.Ok(true);
    }
}
