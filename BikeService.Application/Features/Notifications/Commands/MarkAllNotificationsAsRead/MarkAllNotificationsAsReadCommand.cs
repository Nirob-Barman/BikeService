using BikeService.Application.Wrappers;
using MediatR;

namespace BikeService.Application.Features.Notifications.Commands.MarkAllNotificationsAsRead;

public record MarkAllNotificationsAsReadCommand : IRequest<Result<bool>>;
