using BikeService.Application.DTOs.Notification;
using BikeService.Application.Wrappers;

namespace BikeService.Application.Interfaces
{
    public interface INotificationService
    {
        Task CreateNotificationAsync(string userId, string title, string message, string? link = null);
        Task MarkAsReadAsync(int notificationId);
        Task MarkAllAsReadAsync(string userId);
        Task<int> GetUnreadCountAsync(string userId);
        Task<Result<List<AppNotificationDto>>> GetNotificationsAsync(string userId, int count = 20);
    }
}
