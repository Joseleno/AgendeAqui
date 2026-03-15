using AgendeAqui.Domain.InAppNotifications;

namespace AgendeAqui.Domain.Abstractions;

public interface IInAppNotificationRepository : IRepository<InAppNotification>
{
    Task<int> GetUnreadCountAsync(Guid userId, CancellationToken ct = default);
    Task MarkAllAsReadAsync(Guid userId, CancellationToken ct = default);
}
