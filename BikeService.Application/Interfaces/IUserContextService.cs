namespace BikeService.Application.Interfaces
{
    public interface IUserContextService
    {
        string? UserId { get; }
        string? Email { get; }
        bool IsAuthenticated { get; }
        bool IsInRole(string role);
        string IpAddress { get; }
        string UserAgent { get; }
        string GetBaseUrl();
    }
}
