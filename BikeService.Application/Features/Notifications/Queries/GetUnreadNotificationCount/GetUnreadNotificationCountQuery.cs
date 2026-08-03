using MediatR;

namespace BikeService.Application.Features.Notifications.Queries.GetUnreadNotificationCount;

public record GetUnreadNotificationCountQuery : IRequest<int>;
