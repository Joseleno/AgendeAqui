namespace AgendeAqui.Application.InAppNotifications;

public sealed record InAppNotificationResponse(
    Guid Id,
    string Title,
    string Message,
    string Type,
    Guid? ReferenceId,
    bool IsRead,
    DateTime CreatedAt);
