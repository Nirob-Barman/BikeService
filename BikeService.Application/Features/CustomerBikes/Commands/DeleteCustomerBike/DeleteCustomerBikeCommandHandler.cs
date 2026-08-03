using BikeService.Application.Interfaces;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;
using MediatR;

namespace BikeService.Application.Features.CustomerBikes.Commands.DeleteCustomerBike;

public class DeleteCustomerBikeCommandHandler(
    IUnitOfWork unitOfWork,
    IUserContextService userContextService) : IRequestHandler<DeleteCustomerBikeCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(DeleteCustomerBikeCommand request, CancellationToken cancellationToken)
    {
        var bike = await unitOfWork.Repository<CustomerBike>().GetByIdAsync(request.Id);
        if (bike is null)
            return Result<bool>.Fail("Bike not found.");

        var userId = userContextService.UserId;
        if (!userContextService.IsInRole("Admin") && bike.CustomerId != userId)
            return Result<bool>.Fail("You do not have permission to delete this bike.");

        var hasActiveTickets = await unitOfWork.Repository<ServiceTicket>()
            .AnyAsync(t => t.BikeId == request.Id &&
                           t.Status != Domain.Enums.ServiceTicketStatus.Delivered &&
                           t.Status != Domain.Enums.ServiceTicketStatus.Cancelled);

        if (hasActiveTickets)
            return Result<bool>.Fail("Cannot delete bike because it has active service tickets.");

        unitOfWork.Repository<CustomerBike>().Remove(bike);
        await unitOfWork.SaveChangesAsync();

        return Result<bool>.Ok(true, "Bike deleted successfully.");
    }
}
