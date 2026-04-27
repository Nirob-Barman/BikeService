using System.Text.Json;
using BikeService.Application.DTOs.Customer;
using BikeService.Application.Interfaces;
using BikeService.Application.Interfaces.Identity;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Interfaces.Services;
using BikeService.Application.Wrappers;
using BikeService.Domain.Constants;

namespace BikeService.Application.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly IUserManager _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditLogService _auditLogService;
        private readonly IUserContextService _userContextService;

        public CustomerService(
            IUserManager userManager,
            IUnitOfWork unitOfWork,
            IAuditLogService auditLogService,
            IUserContextService userContextService)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _auditLogService = auditLogService;
            _userContextService = userContextService;
        }

        public async Task<Result<List<CustomerDto>>> GetAllAsync()
        {
            var allUsers = await _userManager.GetAllUsersAsync();
            var customers = new List<CustomerDto>();

            foreach (var user in allUsers)
            {
                if (await _userManager.IsUserInRoleAsync(user, AppRoles.Customer))
                {
                    customers.Add(MapToDto(user));
                }
            }

            return Result<List<CustomerDto>>.Ok(customers);
        }

        public async Task<Result<CustomerDto>> GetByIdAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user is null)
                return Result<CustomerDto>.Fail("Customer not found.");

            if (!await _userManager.IsUserInRoleAsync(user, AppRoles.Customer))
                return Result<CustomerDto>.Fail("User is not a customer.");

            return Result<CustomerDto>.Ok(MapToDto(user));
        }

        public async Task<Result<bool>> BanAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user is null)
                return Result<bool>.Fail("Customer not found.");

            if (user.IsBanned)
                return Result<bool>.Fail("Customer is already banned.");

            var oldValues = JsonSerializer.Serialize(new { user.IsBanned });

            var (succeeded, errors) = await _userManager.SetLockoutAsync(id, true);
            if (!succeeded)
                return Result<bool>.Fail(errors?.FirstOrDefault() ?? "Failed to ban customer.");

            await _auditLogService.LogAsync(
                "Customer", "Ban",
                _userContextService.UserId, _userContextService.Email,
                $"Banned customer '{user.Email}'",
                entityId: id,
                ipAddress: _userContextService.IpAddress,
                userAgent: _userContextService.UserAgent,
                oldValues: oldValues,
                newValues: JsonSerializer.Serialize(new { IsBanned = true }));

            return Result<bool>.Ok(true, "Customer banned successfully.");
        }

        public async Task<Result<bool>> UnbanAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user is null)
                return Result<bool>.Fail("Customer not found.");

            if (!user.IsBanned)
                return Result<bool>.Fail("Customer is not currently banned.");

            var oldValues = JsonSerializer.Serialize(new { user.IsBanned });

            var (succeeded, errors) = await _userManager.SetLockoutAsync(id, false);
            if (!succeeded)
                return Result<bool>.Fail(errors?.FirstOrDefault() ?? "Failed to unban customer.");

            await _auditLogService.LogAsync(
                "Customer", "Unban",
                _userContextService.UserId, _userContextService.Email,
                $"Unbanned customer '{user.Email}'",
                entityId: id,
                ipAddress: _userContextService.IpAddress,
                userAgent: _userContextService.UserAgent,
                oldValues: oldValues,
                newValues: JsonSerializer.Serialize(new { IsBanned = false }));

            return Result<bool>.Ok(true, "Customer unbanned successfully.");
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
}
