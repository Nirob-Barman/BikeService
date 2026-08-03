using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;
using MediatR;

namespace BikeService.Application.Features.Notifications.Commands.MarkNotificationAsRead;

public class MarkNotificationAsReadCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<MarkNotificationAsReadCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(MarkNotificationAsReadCommand request, CancellationToken cancellationToken)
    {
        var notification = await unitOfWork.Repository<AppNotification>().GetByIdAsync(request.NotificationId);
        if (notification == null)
            return Result<bool>.Ok(true);

        notification.IsRead = true;
        unitOfWork.Repository<AppNotification>().Update(notification);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<bool>.Ok(true);
    }
}
