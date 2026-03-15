using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Common;

namespace AgendeAqui.Application.InAppNotifications.MarkAllAsRead;

public sealed class MarkAllAsReadCommandHandler(
    IInAppNotificationRepository notificationRepository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser) : ICommandHandler<MarkAllAsReadCommand>
{
    public async ValueTask<Result<Mediator.Unit>> Handle(
        MarkAllAsReadCommand command,
        CancellationToken cancellationToken)
    {
        await notificationRepository.MarkAllAsReadAsync(currentUser.UserId, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(Mediator.Unit.Value);
    }
}
