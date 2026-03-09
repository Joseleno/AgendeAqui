namespace AgendeAqui.Application.Notifications.ListNotifications;

public sealed record NotificationResponse(
    Guid Id,
    Guid AppointmentId,
    string Channel,
    string Recipient,
    string TemplateName,
    string Status,
    DateTime? SentAt,
    string? ErrorMessage,
    DateTime CreatedAt);
