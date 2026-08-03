using BikeService.Application.Interfaces;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;
using MediatR;

namespace BikeService.Application.Features.Parts.Commands.ResolveStockAlert;

public class ResolveStockAlertCommandHandler(
    IUnitOfWork unitOfWork,
    IUserContextService userContextService) : IRequestHandler<ResolveStockAlertCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(ResolveStockAlertCommand request, CancellationToken cancellationToken)
    {
        var alert = await unitOfWork.Repository<PartStockAlert>().GetByIdAsync(request.Id);
        if (alert is null)
            return Result<bool>.Fail("Stock alert not found.");

        if (alert.IsResolved)
            return Result<bool>.Fail("Stock alert is already resolved.");

        alert.IsResolved = true;
        alert.UpdatedAt = DateTime.UtcNow;
        alert.UpdatedBy = userContextService.UserId;

        unitOfWork.Repository<PartStockAlert>().Update(alert);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<bool>.Ok(true, "Stock alert resolved.");
    }
}
