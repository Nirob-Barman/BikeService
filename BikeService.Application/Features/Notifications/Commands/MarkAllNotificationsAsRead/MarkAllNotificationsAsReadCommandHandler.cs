using BikeService.Application.Interfaces;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;
using MediatR;

namespace BikeService.Application.Features.Notifications.Commands.MarkAllNotificationsAsRead;

public class MarkAllNotificationsAsReadCommandHandler(
    IUnitOfWork unitOfWork,
    IUserContextService userContextService) : IRequestHandler<MarkAllNotificationsAsReadCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(MarkAllNotificationsAsReadCommand request, CancellationToken cancellationToken)
    {
        var userId = userContextService.UserId;
        if (string.IsNullOrEmpty(userId))
            return Result<bool>.Fail("User is not authenticated.");

        var notifications = await unitOfWork.Repository<AppNotification>()
            .Where(n => n.UserId == userId && !n.IsRead);

        foreach (var n in notifications)
            n.IsRead = true;

        unitOfWork.Repository<AppNotification>().UpdateRange(notifications);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<bool>.Ok(true);
    }
}
