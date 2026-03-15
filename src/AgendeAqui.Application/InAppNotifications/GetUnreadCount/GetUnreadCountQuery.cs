using AgendeAqui.Application.Abstractions.Messaging;

namespace AgendeAqui.Application.InAppNotifications.GetUnreadCount;

public sealed record GetUnreadCountQuery() : IQuery<int>;
