using BikeService.Domain.Entities;

namespace BikeService.Application.Interfaces.Identity
{
    public interface ISignInManager
    {
        Task<bool> CheckPasswordSignInAsync(AppUser user, string password);
        Task SignInAsync(AppUser user, bool isPersistent);
        Task SignOutAsync();
        Task RefreshSignInAsync(AppUser user);
    }
}
