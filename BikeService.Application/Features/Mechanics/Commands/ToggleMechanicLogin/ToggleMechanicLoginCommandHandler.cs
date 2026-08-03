using System.Text.Json;
using BikeService.Application.Interfaces;
using BikeService.Application.Interfaces.Identity;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;
using MediatR;

namespace BikeService.Application.Features.Mechanics.Commands.ToggleMechanicLogin;

public class ToggleMechanicLoginCommandHandler(
    IUnitOfWork unitOfWork,
    IAuditLogService auditLogService,
    IUserContextService userContextService,
    IUserManager userManager) : IRequestHandler<ToggleMechanicLoginCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(ToggleMechanicLoginCommand request, CancellationToken cancellationToken)
    {
        var entity = await unitOfWork.Repository<Mechanic>().GetByIdAsync(request.MechanicId);
        if (entity == null)
            return Result<bool>.Fail("Mechanic not found.");

        if (string.IsNullOrEmpty(entity.UserId))
            return Result<bool>.Fail("This mechanic has no login account.");

        var user = await userManager.FindByIdAsync(entity.UserId);
        if (user == null)
            return Result<bool>.Fail("Linked user account not found.");

        var deactivate = !user.IsBanned;
        var (succeeded, errors) = await userManager.SetLockoutAsync(entity.UserId, deactivate);
        if (!succeeded)
            return Result<bool>.Fail(errors.FirstOrDefault() ?? "Failed to update login status.");

        await auditLogService.LogAsync(
            "Mechanic", deactivate ? "DeactivateLogin" : "ActivateLogin",
            userContextService.UserId, userContextService.Email,
            $"{(deactivate ? "Deactivated" : "Activated")} login for mechanic '{entity.FullName}'",
            entityId: request.MechanicId.ToString(),
            ipAddress: userContextService.IpAddress,
            userAgent: userContextService.UserAgent,
            oldValues: JsonSerializer.Serialize(new { IsLoginActive = !deactivate }),
            newValues: JsonSerializer.Serialize(new { IsLoginActive = deactivate }));

        return Result<bool>.Ok(true, deactivate ? "Login deactivated." : "Login activated.");
    }
}
