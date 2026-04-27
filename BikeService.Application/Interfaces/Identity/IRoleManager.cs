
namespace BikeService.Application.Interfaces.Identity
{
    public interface IRoleManager
    {
        Task<List<string>> GetAllRolesAsync();
        Task<List<string>> GetAllRolesAsync(bool excludeAdmin = true);
        Task<bool> RoleExistsAsync(string roleName);
        Task<(bool Succeeded, List<string> Errors)> CreateRoleAsync(string roleName);
        Task<(bool Succeeded, List<string> Errors)> DeleteRoleAsync(string roleName);
    }
}
