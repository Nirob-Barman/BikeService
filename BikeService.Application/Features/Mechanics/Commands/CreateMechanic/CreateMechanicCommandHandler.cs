using System.Text.Json;
using BikeService.Application.Interfaces;
using BikeService.Application.Interfaces.Identity;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Wrappers;
using BikeService.Domain.Constants;
using BikeService.Domain.Entities;
using MediatR;

namespace BikeService.Application.Features.Mechanics.Commands.CreateMechanic;

public class CreateMechanicCommandHandler(
    IUnitOfWork unitOfWork,
    IAuditLogService auditLogService,
    IUserContextService userContextService,
    IUserManager userManager) : IRequestHandler<CreateMechanicCommand, Result<int>>
{
    public async Task<Result<int>> Handle(CreateMechanicCommand request, CancellationToken cancellationToken)
    {
        var duplicate = await unitOfWork.Repository<Mechanic>()
            .AnyAsync(e => e.FullName == request.FullName);
        if (duplicate)
            return Result<int>.FailField("FullName", "A mechanic with this name already exists.");

        string? userId = null;
        if (!string.IsNullOrWhiteSpace(request.Email) && !string.IsNullOrWhiteSpace(request.Password))
        {
            var existing = await userManager.FindByEmailAsync(request.Email);
            if (existing != null)
                return Result<int>.FailField("Email", "An account with this email already exists.");

            var nameParts = request.FullName.Trim().Split(' ', 2);
            var newUser = new AppUser
            {
                Email = request.Email,
                FirstName = nameParts[0],
                LastName = nameParts.Length > 1 ? nameParts[1] : string.Empty,
            };

            var (created, createdId, createErrors) = await userManager.CreateAsync(newUser, request.Password);
            if (!created)
                return Result<int>.Fail(createErrors.FirstOrDefault() ?? "Failed to create login account.");

            var createdUser = await userManager.FindByEmailAsync(request.Email);
            var (roleAdded, roleErrors) = await userManager.AddToRoleAsync(createdUser!, AppRoles.Mechanic);
            if (!roleAdded)
                return Result<int>.Fail(roleErrors.FirstOrDefault() ?? "Failed to assign Mechanic role.");

            userId = createdId;
        }

        var entity = new Mechanic
        {
            FullName = request.FullName,
            Specialty = request.Specialty,
            IsAvailable = request.IsAvailable,
            UserId = userId,
            CreatedBy = userContextService.UserId,
            CreatedAt = DateTime.UtcNow
        };

        await unitOfWork.Repository<Mechanic>().AddAsync(entity);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await auditLogService.LogAsync(
            "Mechanic", "Create",
            userContextService.UserId, userContextService.Email,
            $"Created mechanic '{entity.FullName}'",
            entityId: entity.Id.ToString(),
            ipAddress: userContextService.IpAddress,
            userAgent: userContextService.UserAgent,
            oldValues: null,
            newValues: JsonSerializer.Serialize(new
            {
                entity.FullName,
                entity.Specialty,
                entity.IsAvailable,
                entity.UserId
            }));

        return Result<int>.Ok(entity.Id, "Mechanic created successfully.");
    }
}
