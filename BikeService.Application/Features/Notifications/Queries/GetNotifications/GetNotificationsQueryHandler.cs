using BikeService.Application.DTOs.Notification;
using BikeService.Application.Interfaces;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;
using MediatR;

namespace BikeService.Application.Features.Notifications.Queries.GetNotifications;

public class GetNotificationsQueryHandler(
    IUnitOfWork unitOfWork,
    IUserContextService userContextService) : IRequestHandler<GetNotificationsQuery, Result<List<AppNotificationDto>>>
{
    public async Task<Result<List<AppNotificationDto>>> Handle(GetNotificationsQuery request, CancellationToken cancellationToken)
    {
        var userId = userContextService.UserId;
        if (string.IsNullOrEmpty(userId))
            return Result<List<AppNotificationDto>>.Fail("User is not authenticated.");

        var notifications = await unitOfWork.Repository<AppNotification>()
            .Where(n => n.UserId == userId);

        var dtos = notifications
            .OrderByDescending(n => n.CreatedAt)
            .Take(request.Count)
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
