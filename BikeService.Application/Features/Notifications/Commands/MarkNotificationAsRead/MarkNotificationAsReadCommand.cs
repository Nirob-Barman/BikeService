using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.Notifications.Commands.MarkNotificationAsRead;

public record MarkNotificationAsReadCommand(int NotificationId) : IRequest<Result<bool>>;
