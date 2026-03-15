using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Common;
using AgendeAqui.Domain.InAppNotifications;

namespace AgendeAqui.Application.InAppNotifications.MarkAsRead;

public sealed class MarkAsReadCommandHandler(
    IInAppNotificationRepository notificationRepository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser) : ICommandHandler<MarkAsReadCommand>
{
    public async ValueTask<Result<Mediator.Unit>> Handle(
        MarkAsReadCommand command,
        CancellationToken cancellationToken)
    {
        var notification = await notificationRepository.GetByIdAsync(command.Id, cancellationToken);

        if (notification is null)
            return Result.Failure<Mediator.Unit>(InAppNotificationErrors.NotFound);

        if (notification.UserId != currentUser.UserId)
            return Result.Failure<Mediator.Unit>(InAppNotificationErrors.NotAuthorized);

        notification.MarkAsRead();
        notificationRepository.Update(notification);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(Mediator.Unit.Value);
    }
}
