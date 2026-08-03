using BikeService.Application.DTOs.Customer;
using BikeService.Application.Interfaces.Identity;
using BikeService.Application.Wrappers;
using BikeService.Domain.Constants;
using MediatR;

namespace BikeService.Application.Features.Customers.Queries.GetCustomerById;

public class GetCustomerByIdQueryHandler(IUserManager userManager)
    : IRequestHandler<GetCustomerByIdQuery, Result<CustomerDto>>
{
    public async Task<Result<CustomerDto>> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(request.Id);
        if (user is null)
            return Result<CustomerDto>.Fail("Customer not found.");

        if (!await userManager.IsUserInRoleAsync(user, AppRoles.Customer))
            return Result<CustomerDto>.Fail("User is not a customer.");

        return Result<CustomerDto>.Ok(MapToDto(user));
    }

    private static CustomerDto MapToDto(Domain.Entities.AppUser user) => new()
    {
        Id = user.Id ?? string.Empty,
        Email = user.Email ?? string.Empty,
        FirstName = user.FirstName ?? string.Empty,
        LastName = user.LastName ?? string.Empty,
        FullName = user.FullName,
        IsBanned = user.IsBanned
    };
}
