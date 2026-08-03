using System.Text.Json;
using BikeService.Application.Interfaces;
using BikeService.Application.Interfaces.Identity;
using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.Customers.Commands.UnbanCustomer;

public class UnbanCustomerCommandHandler(
    IUserManager userManager,
    IAuditLogService auditLogService,
    IUserContextService userContextService) : IRequestHandler<UnbanCustomerCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(UnbanCustomerCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(request.Id);
        if (user is null)
            return Result<bool>.Fail("Customer not found.");

        if (!user.IsBanned)
            return Result<bool>.Fail("Customer is not currently banned.");

        var oldValues = JsonSerializer.Serialize(new { user.IsBanned });

        var (succeeded, errors) = await userManager.SetLockoutAsync(request.Id, false);
        if (!succeeded)
            return Result<bool>.Fail(errors?.FirstOrDefault() ?? "Failed to unban customer.");

        await auditLogService.LogAsync(
            "Customer", "Unban",
            userContextService.UserId, userContextService.Email,
            $"Unbanned customer '{user.Email}'",
            entityId: request.Id,
            ipAddress: userContextService.IpAddress,
            userAgent: userContextService.UserAgent,
            oldValues: oldValues,
            newValues: JsonSerializer.Serialize(new { IsBanned = false }));

        return Result<bool>.Ok(true, "Customer unbanned successfully.");
    }
}
