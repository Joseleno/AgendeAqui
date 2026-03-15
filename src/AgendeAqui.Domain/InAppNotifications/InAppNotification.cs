using AgendeAqui.Domain.Common;

namespace AgendeAqui.Domain.InAppNotifications;

public sealed class InAppNotification : TenantEntity
{
    public Guid UserId { get; private set; }
    public string Title { get; private set; } = default!;
    public string Message { get; private set; } = default!;
    public InAppNotificationType Type { get; private set; }
    public Guid? ReferenceId { get; private set; }
    public bool IsRead { get; private set; }

    private InAppNotification() { }

    public static InAppNotification Create(
        Guid tenantId,
        Guid userId,
        string title,
        string message,
        InAppNotificationType type,
        Guid? referenceId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        return new InAppNotification
        {
            TenantId = tenantId,
            UserId = userId,
            Title = title,
            Message = message,
            Type = type,
            ReferenceId = referenceId,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void MarkAsRead()
    {
        IsRead = true;
        UpdatedAt = DateTime.UtcNow;
    }
}
