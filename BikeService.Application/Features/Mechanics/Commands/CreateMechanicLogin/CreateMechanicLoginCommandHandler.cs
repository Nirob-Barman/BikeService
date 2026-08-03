using System.Text.Json;
using BikeService.Application.Interfaces;
using BikeService.Application.Interfaces.Identity;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Wrappers;
using BikeService.Domain.Constants;
using BikeService.Domain.Entities;
using MediatR;

namespace BikeService.Application.Features.Mechanics.Commands.CreateMechanicLogin;

public class CreateMechanicLoginCommandHandler(
    IUnitOfWork unitOfWork,
    IAuditLogService auditLogService,
    IUserContextService userContextService,
    IUserManager userManager) : IRequestHandler<CreateMechanicLoginCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(CreateMechanicLoginCommand request, CancellationToken cancellationToken)
    {
        var entity = await unitOfWork.Repository<Mechanic>().GetByIdAsync(request.MechanicId);
        if (entity == null)
            return Result<bool>.Fail("Mechanic not found.");

        if (!string.IsNullOrEmpty(entity.UserId))
            return Result<bool>.Fail("This mechanic already has a login account.");

        var existing = await userManager.FindByEmailAsync(request.Email);
        if (existing != null)
            return Result<bool>.FailField("Email", "An account with this email already exists.");

        var nameParts = entity.FullName.Trim().Split(' ', 2);
        var newUser = new AppUser
        {
            Email = request.Email,
            FirstName = nameParts[0],
            LastName = nameParts.Length > 1 ? nameParts[1] : string.Empty,
        };

        var (created, userId, createErrors) = await userManager.CreateAsync(newUser, request.Password);
        if (!created)
            return Result<bool>.Fail(createErrors.FirstOrDefault() ?? "Failed to create login account.");

        var createdUser = await userManager.FindByEmailAsync(request.Email);
        var (roleAdded, roleErrors) = await userManager.AddToRoleAsync(createdUser!, AppRoles.Mechanic);
        if (!roleAdded)
            return Result<bool>.Fail(roleErrors.FirstOrDefault() ?? "Failed to assign Mechanic role.");

        entity.UserId = userId;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = userContextService.UserId;
        unitOfWork.Repository<Mechanic>().Update(entity);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await auditLogService.LogAsync(
            "Mechanic", "CreateLogin",
            userContextService.UserId, userContextService.Email,
            $"Created login account '{request.Email}' for mechanic '{entity.FullName}'",
            entityId: request.MechanicId.ToString(),
            ipAddress: userContextService.IpAddress,
            userAgent: userContextService.UserAgent,
            oldValues: JsonSerializer.Serialize(new { UserId = (string?)null }),
            newValues: JsonSerializer.Serialize(new { UserId = userId, Email = request.Email }));

        return Result<bool>.Ok(true, "Login account created successfully.");
    }
}
