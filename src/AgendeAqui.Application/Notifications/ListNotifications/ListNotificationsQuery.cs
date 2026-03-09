using AgendeAqui.Application.Abstractions.Messaging;

namespace AgendeAqui.Application.Notifications.ListNotifications;

public sealed record ListNotificationsQuery(
    Guid? AppointmentId = null,
    DateOnly? DateFrom = null,
    DateOnly? DateTo = null) : IQuery<List<NotificationResponse>>;
