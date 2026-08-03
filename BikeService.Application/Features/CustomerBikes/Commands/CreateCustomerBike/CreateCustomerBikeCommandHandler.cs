using BikeService.Application.Interfaces;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;
using MediatR;

namespace BikeService.Application.Features.CustomerBikes.Commands.CreateCustomerBike;

public class CreateCustomerBikeCommandHandler(
    IUnitOfWork unitOfWork,
    IUserContextService userContextService) : IRequestHandler<CreateCustomerBikeCommand, Result<int>>
{
    public async Task<Result<int>> Handle(CreateCustomerBikeCommand request, CancellationToken cancellationToken)
    {
        var userId = userContextService.UserId;
        if (string.IsNullOrEmpty(userId))
            return Result<int>.Fail("User is not authenticated.");

        var bike = new CustomerBike
        {
            Make = request.Make,
            Model = request.Model,
            Year = request.Year,
            RegistrationNo = request.RegistrationNo,
            ImageUrl = request.ImageUrl
        };
        bike.CustomerId = userId;
        bike.CreatedBy = userId;

        await unitOfWork.Repository<CustomerBike>().AddAsync(bike);
        await unitOfWork.SaveChangesAsync();

        return Result<int>.Ok(bike.Id, "Bike registered successfully.");
    }
}
