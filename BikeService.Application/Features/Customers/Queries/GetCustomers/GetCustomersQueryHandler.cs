using BikeService.Application.DTOs.Customer;
using BikeService.Application.Interfaces.Identity;
using BikeService.Application.Wrappers;
using BikeService.Domain.Constants;
using MediatR;

namespace BikeService.Application.Features.Customers.Queries.GetCustomers;

public class GetCustomersQueryHandler(IUserManager userManager)
    : IRequestHandler<GetCustomersQuery, Result<List<CustomerDto>>>
{
    public async Task<Result<List<CustomerDto>>> Handle(GetCustomersQuery request, CancellationToken cancellationToken)
    {
        var allUsers = await userManager.GetAllUsersAsync();
        var customers = new List<CustomerDto>();

        foreach (var user in allUsers)
        {
            if (await userManager.IsUserInRoleAsync(user, AppRoles.Customer))
            {
                customers.Add(MapToDto(user));
            }
        }

        return Result<List<CustomerDto>>.Ok(customers);
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
