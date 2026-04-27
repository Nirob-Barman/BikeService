using BikeService.Application.DTOs.Notification;
using BikeService.Application.Interfaces;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;

namespace BikeService.Application.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _unitOfWork;

        public NotificationService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task CreateNotificationAsync(string userId, string title, string message, string? link = null)
        {
            var notification = new AppNotification
            {
                UserId = userId,
                Title = title,
                Message = message,
                Link = link,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<AppNotification>().AddAsync(notification);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task MarkAsReadAsync(int notificationId)
        {
            var notification = await _unitOfWork.Repository<AppNotification>().GetByIdAsync(notificationId);
            if (notification == null) return;

            notification.IsRead = true;
            _unitOfWork.Repository<AppNotification>().Update(notification);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task MarkAllAsReadAsync(string userId)
        {
            var notifications = await _unitOfWork.Repository<AppNotification>()
                .Where(n => n.UserId == userId && !n.IsRead);

            foreach (var n in notifications)
                n.IsRead = true;

            _unitOfWork.Repository<AppNotification>().UpdateRange(notifications);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<int> GetUnreadCountAsync(string userId)
        {
            return await _unitOfWork.Repository<AppNotification>()
                .CountAsync(n => n.UserId == userId && !n.IsRead);
        }

        public async Task<Result<List<AppNotificationDto>>> GetNotificationsAsync(string userId, int count = 20)
        {
            var notifications = await _unitOfWork.Repository<AppNotification>()
                .Where(n => n.UserId == userId);

            var dtos = notifications
                .OrderByDescending(n => n.CreatedAt)
                .Take(count)
                .Select(n => new AppNotificationDto
                {
                    Id = n.Id,
                    Title = n.Title,
                    Message = n.Message,
                    Link = n.Link,
                    IsRead = n.IsRead,
                    CreatedAt = n.CreatedAt
                })
                .ToList();

            return Result<List<AppNotificationDto>>.Ok(dtos);
        }
    }
}
