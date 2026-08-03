using BikeService.Application.Interfaces;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;
using MediatR;

namespace BikeService.Application.Features.CustomerBikes.Commands.UpdateCustomerBike;

public class UpdateCustomerBikeCommandHandler(
    IUnitOfWork unitOfWork,
    IUserContextService userContextService) : IRequestHandler<UpdateCustomerBikeCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(UpdateCustomerBikeCommand request, CancellationToken cancellationToken)
    {
        var bike = await unitOfWork.Repository<CustomerBike>().GetByIdAsync(request.Id);
        if (bike is null)
            return Result<bool>.Fail("Bike not found.");

        var userId = userContextService.UserId;
        if (!userContextService.IsInRole("Admin") && bike.CustomerId != userId)
            return Result<bool>.Fail("You do not have permission to update this bike.");

        bike.Make = request.Make;
        bike.Model = request.Model;
        bike.Year = request.Year;
        bike.RegistrationNo = request.RegistrationNo;
        bike.ImageUrl = request.ImageUrl;
        bike.UpdatedAt = DateTime.UtcNow;
        bike.UpdatedBy = userId;

        unitOfWork.Repository<CustomerBike>().Update(bike);
        await unitOfWork.SaveChangesAsync();

        return Result<bool>.Ok(true, "Bike updated successfully.");
    }
}
