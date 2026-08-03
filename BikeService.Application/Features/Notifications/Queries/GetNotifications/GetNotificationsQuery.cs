using BikeService.Application.DTOs.Notification;
using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.Notifications.Queries.GetNotifications;

public record GetNotificationsQuery(int Count = 20) : IRequest<Result<List<AppNotificationDto>>>;
