using System.Text.Json;
using BikeService.Application.Interfaces;
using BikeService.Application.Interfaces.Identity;
using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.Customers.Commands.BanCustomer;

public class BanCustomerCommandHandler(
    IUserManager userManager,
    IAuditLogService auditLogService,
    IUserContextService userContextService) : IRequestHandler<BanCustomerCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(BanCustomerCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(request.Id);
        if (user is null)
            return Result<bool>.Fail("Customer not found.");

        if (user.IsBanned)
            return Result<bool>.Fail("Customer is already banned.");

        var oldValues = JsonSerializer.Serialize(new { user.IsBanned });

        var (succeeded, errors) = await userManager.SetLockoutAsync(request.Id, true);
        if (!succeeded)
            return Result<bool>.Fail(errors?.FirstOrDefault() ?? "Failed to ban customer.");

        await auditLogService.LogAsync(
            "Customer", "Ban",
            userContextService.UserId, userContextService.Email,
            $"Banned customer '{user.Email}'",
            entityId: request.Id,
            ipAddress: userContextService.IpAddress,
            userAgent: userContextService.UserAgent,
            oldValues: oldValues,
            newValues: JsonSerializer.Serialize(new { IsBanned = true }));

        return Result<bool>.Ok(true, "Customer banned successfully.");
    }
}
