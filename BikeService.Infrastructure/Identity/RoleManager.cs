using BikeService.Application.Interfaces.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BikeService.Infrastructure.Identity
{
    public class RoleManager : IRoleManager
    {
        private readonly RoleManager<IdentityRole> _roleManager;

        public RoleManager(RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
        }

        public async Task<List<string>> GetAllRolesAsync()
        {
            //return await _roleManager.Roles.Where(r => r.Name != "Admin").Select(r => r.Name!).ToListAsync();
            return await _roleManager.Roles.Select(r => r.Name!).ToListAsync();
        }

        public async Task<List<string>> GetAllRolesAsync(bool excludeAdmin = true)
        {
            var roles = _roleManager.Roles.AsQueryable();

            if (excludeAdmin)
                roles = roles.Where(r => r.Name != "Admin");

            return await roles.Select(r => r.Name!).ToListAsync();
        }



        public async Task<bool> RoleExistsAsync(string roleName)
        {
            return await _roleManager.RoleExistsAsync(roleName);
        }

        public async Task<(bool Succeeded, List<string> Errors)> CreateRoleAsync(string roleName)
        {
            if (await _roleManager.RoleExistsAsync(roleName))
                return (false, new List<string> { "Role already exists." });

            var role = new IdentityRole(roleName);
            var result = await _roleManager.CreateAsync(role);

            return (result.Succeeded, result.Errors.Select(e => e.Description).ToList());
        }

        public async Task<(bool Succeeded, List<string> Errors)> DeleteRoleAsync(string roleName)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role == null)
                return (false, new List<string> { "Role does not exist." });

            var result = await _roleManager.DeleteAsync(role);

            return (result.Succeeded, result.Errors.Select(e => e.Description).ToList());
        }
    }
}
