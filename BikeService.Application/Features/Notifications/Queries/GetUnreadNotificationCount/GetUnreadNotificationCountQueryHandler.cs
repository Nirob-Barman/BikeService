using BikeService.Application.Interfaces;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Domain.Entities;
using MediatR;

namespace BikeService.Application.Features.Notifications.Queries.GetUnreadNotificationCount;

public class GetUnreadNotificationCountQueryHandler(
    IUnitOfWork unitOfWork,
    IUserContextService userContextService) : IRequestHandler<GetUnreadNotificationCountQuery, int>
{
    public async Task<int> Handle(GetUnreadNotificationCountQuery request, CancellationToken cancellationToken)
    {
        var userId = userContextService.UserId;
        if (string.IsNullOrEmpty(userId))
            return 0;

        return await unitOfWork.Repository<AppNotification>()
            .CountAsync(n => n.UserId == userId && !n.IsRead);
    }
}
