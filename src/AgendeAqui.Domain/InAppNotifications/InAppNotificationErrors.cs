using AgendeAqui.Domain.Common;

namespace AgendeAqui.Domain.InAppNotifications;

public static class InAppNotificationErrors
{
    public static readonly Error NotFound = new("InAppNotification.NotFound", "Notification not found.");
    public static readonly Error NotAuthorized = new("InAppNotification.NotAuthorized", "You are not authorized to manage this notification.");
}
