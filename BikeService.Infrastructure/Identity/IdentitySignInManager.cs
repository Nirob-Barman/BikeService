using BikeService.Application.Interfaces.Identity;
using BikeService.Domain.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;

namespace BikeService.Infrastructure.Identity
{
    public class IdentitySignInManager : ISignInManager
    {
        private readonly SignInManager<ApplicationUser> _signInManager;

        public IdentitySignInManager(SignInManager<ApplicationUser> signInManager)
        {
            _signInManager = signInManager;
        }

        public async Task<bool> CheckPasswordSignInAsync(AppUser user, string password)
        {
            var identityUser = await _signInManager.UserManager.FindByIdAsync(user.Id!.ToString());
            if (identityUser == null) return false;

            var result = await _signInManager.CheckPasswordSignInAsync(identityUser, password, lockoutOnFailure: true);
            return result.Succeeded;
        }

        public async Task SignInAsync(AppUser user, bool isPersistent)
        {
            var identityUser = await _signInManager.UserManager.FindByIdAsync(user.Id!.ToString());
            if (identityUser == null) return;

            var props = new AuthenticationProperties
            {
                IsPersistent = isPersistent,
                ExpiresUtc = isPersistent
                    ? DateTimeOffset.UtcNow.AddDays(30)   // Remember Me → 30 days
                    : DateTimeOffset.UtcNow.AddMinutes(30) // Session → 30 min inactivity
            };

            await _signInManager.SignInAsync(identityUser, props);
        }


        public async Task SignOutAsync()
        {
            await _signInManager.SignOutAsync();
        }

        public async Task RefreshSignInAsync(AppUser user)
        {
            var identityUser = await _signInManager.UserManager.FindByIdAsync(user.Id!.ToString());
            if (identityUser != null)
            {
                await _signInManager.RefreshSignInAsync(identityUser);
            }
        }
    }
}