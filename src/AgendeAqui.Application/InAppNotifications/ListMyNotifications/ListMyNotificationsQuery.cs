using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Application.Common;

namespace AgendeAqui.Application.InAppNotifications.ListMyNotifications;

public sealed record ListMyNotificationsQuery(
    int Page = 1,
    int PageSize = 20) : IQuery<PagedResponse<InAppNotificationResponse>>;
