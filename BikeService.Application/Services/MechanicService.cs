using System.Text.Json;
using BikeService.Application.DTOs.Mechanic;
using BikeService.Application.Interfaces;
using BikeService.Application.Interfaces.Identity;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Interfaces.Services;
using BikeService.Application.Mappers;
using BikeService.Application.Wrappers;
using BikeService.Domain.Constants;
using BikeService.Domain.Entities;
using BikeService.Domain.Enums;

namespace BikeService.Application.Services
{
    public class MechanicService : IMechanicService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditLogService _auditLogService;
        private readonly IUserContextService _userContextService;
        private readonly IUserManager _userManager;

        public MechanicService(
            IUnitOfWork unitOfWork,
            IAuditLogService auditLogService,
            IUserContextService userContextService,
            IUserManager userManager)
        {
            _unitOfWork = unitOfWork;
            _auditLogService = auditLogService;
            _userContextService = userContextService;
            _userManager = userManager;
        }

        public async Task<Result<List<MechanicDto>>> GetAllAsync()
        {
            var items = await _unitOfWork.Repository<Mechanic>()
                .GetAllAsync<MechanicDto>(e => MechanicMapper.ToDto(e));
            return Result<List<MechanicDto>>.Ok(items.ToList());
        }

        public async Task<Result<MechanicDto>> GetByIdAsync(int id)
        {
            var entity = await _unitOfWork.Repository<Mechanic>().GetByIdAsync(id);
            if (entity == null)
                return Result<MechanicDto>.Fail("Mechanic not found.");

            var dto = MechanicMapper.ToDto(entity);
            if (!string.IsNullOrEmpty(entity.UserId))
            {
                var user = await _userManager.FindByIdAsync(entity.UserId);
                dto.LinkedEmail = user?.Email;
                dto.IsLoginActive = user != null && !user.IsBanned;
            }
            return Result<MechanicDto>.Ok(dto);
        }

        public async Task<Result<List<MechanicDto>>> GetAvailableAsync()
        {
            var items = await _unitOfWork.Repository<Mechanic>()
                .GetAllAsync<MechanicDto>(e => e.IsAvailable, e => MechanicMapper.ToDto(e));
            return Result<List<MechanicDto>>.Ok(items.ToList());
        }

        public async Task<Result<int>> CreateAsync(MechanicFormDto dto)
        {
            var duplicate = await _unitOfWork.Repository<Mechanic>()
                .AnyAsync(e => e.FullName == dto.FullName);
            if (duplicate)
                return Result<int>.FailField("FullName", "A mechanic with this name already exists.");

            // Create login account if email + password provided
            string? userId = null;
            if (!string.IsNullOrWhiteSpace(dto.Email) && !string.IsNullOrWhiteSpace(dto.Password))
            {
                var existing = await _userManager.FindByEmailAsync(dto.Email);
                if (existing != null)
                    return Result<int>.FailField("Email", "An account with this email already exists.");

                var nameParts = dto.FullName.Trim().Split(' ', 2);
                var newUser = new AppUser
                {
                    Email = dto.Email,
                    FirstName = nameParts[0],
                    LastName = nameParts.Length > 1 ? nameParts[1] : string.Empty,
                };

                var (created, createdId, createErrors) = await _userManager.CreateAsync(newUser, dto.Password);
                if (!created)
                    return Result<int>.Fail(createErrors.FirstOrDefault() ?? "Failed to create login account.");

                var createdUser = await _userManager.FindByEmailAsync(dto.Email);
                var (roleAdded, roleErrors) = await _userManager.AddToRoleAsync(createdUser!, AppRoles.Mechanic);
                if (!roleAdded)
                    return Result<int>.Fail(roleErrors.FirstOrDefault() ?? "Failed to assign Mechanic role.");

                userId = createdId;
            }

            var entity = MechanicMapper.ToEntity(dto);
            entity.UserId = userId;
            entity.CreatedBy = _userContextService.UserId;
            entity.CreatedAt = DateTime.UtcNow;

            await _unitOfWork.Repository<Mechanic>().AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            await _auditLogService.LogAsync(
                "Mechanic", "Create",
                _userContextService.UserId, _userContextService.Email,
                $"Created mechanic '{entity.FullName}'",
                entityId: entity.Id.ToString(),
                ipAddress: _userContextService.IpAddress,
                userAgent: _userContextService.UserAgent,
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

        public async Task<Result<bool>> UpdateAsync(int id, MechanicFormDto dto)
        {
            var entity = await _unitOfWork.Repository<Mechanic>().GetByIdAsync(id);
            if (entity == null)
                return Result<bool>.Fail("Mechanic not found.");

            var duplicate = await _unitOfWork.Repository<Mechanic>()
                .AnyAsync(e => e.FullName == dto.FullName && e.Id != id);
            if (duplicate)
                return Result<bool>.FailField("FullName", "A mechanic with this name already exists.");

            var oldValues = JsonSerializer.Serialize(new
            {
                entity.FullName,
                entity.Specialty,
                entity.IsAvailable,
                entity.UserId
            });

            MechanicMapper.UpdateEntity(entity, dto);
            entity.UpdatedBy = _userContextService.UserId;
            entity.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Repository<Mechanic>().Update(entity);
            await _unitOfWork.SaveChangesAsync();

            await _auditLogService.LogAsync(
                "Mechanic", "Update",
                _userContextService.UserId, _userContextService.Email,
                $"Updated mechanic '{entity.FullName}'",
                entityId: entity.Id.ToString(),
                ipAddress: _userContextService.IpAddress,
                userAgent: _userContextService.UserAgent,
                oldValues: oldValues,
                newValues: JsonSerializer.Serialize(new
                {
                    entity.FullName,
                    entity.Specialty,
                    entity.IsAvailable,
                    entity.UserId
                }));

            return Result<bool>.Ok(true, "Mechanic updated successfully.");
        }

        public async Task<Result<bool>> ToggleAvailabilityAsync(int id)
        {
            var entity = await _unitOfWork.Repository<Mechanic>().GetByIdAsync(id);
            if (entity == null)
                return Result<bool>.Fail("Mechanic not found.");

            var oldValues = JsonSerializer.Serialize(new { entity.IsAvailable });

            entity.IsAvailable = !entity.IsAvailable;
            entity.UpdatedBy = _userContextService.UserId;
            entity.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Repository<Mechanic>().Update(entity);
            await _unitOfWork.SaveChangesAsync();

            await _auditLogService.LogAsync(
                "Mechanic", "Toggle",
                _userContextService.UserId, _userContextService.Email,
                $"Toggled mechanic '{entity.FullName}' IsAvailable to {entity.IsAvailable}",
                entityId: entity.Id.ToString(),
                ipAddress: _userContextService.IpAddress,
                userAgent: _userContextService.UserAgent,
                oldValues: oldValues,
                newValues: JsonSerializer.Serialize(new { entity.IsAvailable }));

            return Result<bool>.Ok(true, $"Mechanic is now {(entity.IsAvailable ? "available" : "unavailable")}.");
        }

        public async Task<Result<bool>> DeleteAsync(int id)
        {
            var entity = await _unitOfWork.Repository<Mechanic>().GetByIdAsync(id);
            if (entity == null)
                return Result<bool>.Fail("Mechanic not found.");

            var hasActiveTickets = await _unitOfWork.Repository<ServiceTicket>()
                .AnyAsync(t => t.MechanicId == id
                    && t.Status != ServiceTicketStatus.Delivered
                    && t.Status != ServiceTicketStatus.Cancelled);
            if (hasActiveTickets)
                return Result<bool>.Fail("Mechanic has active tickets and cannot be deleted.");

            var oldValues = JsonSerializer.Serialize(new
            {
                entity.FullName,
                entity.Specialty,
                entity.IsAvailable,
                entity.UserId
            });

            _unitOfWork.Repository<Mechanic>().Remove(entity);
            await _unitOfWork.SaveChangesAsync();

            await _auditLogService.LogAsync(
                "Mechanic", "Delete",
                _userContextService.UserId, _userContextService.Email,
                $"Deleted mechanic '{entity.FullName}'",
                entityId: id.ToString(),
                ipAddress: _userContextService.IpAddress,
                userAgent: _userContextService.UserAgent,
                oldValues: oldValues,
                newValues: null);

            return Result<bool>.Ok(true, "Mechanic deleted successfully.");
        }

        public async Task<Result<bool>> CreateLoginAsync(int mechanicId, string email, string password)
        {
            var entity = await _unitOfWork.Repository<Mechanic>().GetByIdAsync(mechanicId);
            if (entity == null)
                return Result<bool>.Fail("Mechanic not found.");

            if (!string.IsNullOrEmpty(entity.UserId))
                return Result<bool>.Fail("This mechanic already has a login account.");

            var existing = await _userManager.FindByEmailAsync(email);
            if (existing != null)
                return Result<bool>.FailField("Email", "An account with this email already exists.");

            var nameParts = entity.FullName.Trim().Split(' ', 2);
            var newUser = new AppUser
            {
                Email = email,
                FirstName = nameParts[0],
                LastName = nameParts.Length > 1 ? nameParts[1] : string.Empty,
            };

            var (created, userId, createErrors) = await _userManager.CreateAsync(newUser, password);
            if (!created)
                return Result<bool>.Fail(createErrors.FirstOrDefault() ?? "Failed to create login account.");

            var createdUser = await _userManager.FindByEmailAsync(email);
            var (roleAdded, roleErrors) = await _userManager.AddToRoleAsync(createdUser!, AppRoles.Mechanic);
            if (!roleAdded)
                return Result<bool>.Fail(roleErrors.FirstOrDefault() ?? "Failed to assign Mechanic role.");

            entity.UserId = userId;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = _userContextService.UserId;
            _unitOfWork.Repository<Mechanic>().Update(entity);
            await _unitOfWork.SaveChangesAsync();

            await _auditLogService.LogAsync(
                "Mechanic", "CreateLogin",
                _userContextService.UserId, _userContextService.Email,
                $"Created login account '{email}' for mechanic '{entity.FullName}'",
                entityId: mechanicId.ToString(),
                ipAddress: _userContextService.IpAddress,
                userAgent: _userContextService.UserAgent,
                oldValues: JsonSerializer.Serialize(new { UserId = (string?)null }),
                newValues: JsonSerializer.Serialize(new { UserId = userId, Email = email }));

            return Result<bool>.Ok(true, "Login account created successfully.");
        }

        public async Task<Result<bool>> ToggleLoginAsync(int mechanicId)
        {
            var entity = await _unitOfWork.Repository<Mechanic>().GetByIdAsync(mechanicId);
            if (entity == null)
                return Result<bool>.Fail("Mechanic not found.");

            if (string.IsNullOrEmpty(entity.UserId))
                return Result<bool>.Fail("This mechanic has no login account.");

            var user = await _userManager.FindByIdAsync(entity.UserId);
            if (user == null)
                return Result<bool>.Fail("Linked user account not found.");

            var deactivate = !user.IsBanned;
            var (succeeded, errors) = await _userManager.SetLockoutAsync(entity.UserId, deactivate);
            if (!succeeded)
                return Result<bool>.Fail(errors.FirstOrDefault() ?? "Failed to update login status.");

            await _auditLogService.LogAsync(
                "Mechanic", deactivate ? "DeactivateLogin" : "ActivateLogin",
                _userContextService.UserId, _userContextService.Email,
                $"{(deactivate ? "Deactivated" : "Activated")} login for mechanic '{entity.FullName}'",
                entityId: mechanicId.ToString(),
                ipAddress: _userContextService.IpAddress,
                userAgent: _userContextService.UserAgent,
                oldValues: JsonSerializer.Serialize(new { IsLoginActive = !deactivate }),
                newValues: JsonSerializer.Serialize(new { IsLoginActive = deactivate }));

            return Result<bool>.Ok(true, deactivate ? "Login deactivated." : "Login activated.");
        }
    }
}
