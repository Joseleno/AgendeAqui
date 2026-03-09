using AgendeAqui.Application.Abstractions.Messaging;

namespace AgendeAqui.Application.Notifications.ListNotifications;

public sealed record ListNotificationsQuery(Guid AppointmentId) : IQuery<List<NotificationResponse>>;
