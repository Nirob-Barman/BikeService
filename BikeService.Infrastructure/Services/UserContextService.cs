using BikeService.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace BikeService.Infrastructure.Services
{
    public class UserContextService : IUserContextService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserContextService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string? UserId => _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        public string? Email => _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Email);
        public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;
        public bool IsInRole(string role) => _httpContextAccessor.HttpContext?.User.IsInRole(role) ?? false;
        public string IpAddress => _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        public string UserAgent => _httpContextAccessor.HttpContext?.Request.Headers["User-Agent"].ToString() ?? "Unknown";
        public string GetBaseUrl()
        {
            var request = _httpContextAccessor.HttpContext?.Request;
            return request == null ? "" : $"{request.Scheme}://{request.Host}";
        }
    }
}
