using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Application.Abstractions.RealTime;
using AgendeAqui.Application.Appointments.IntegrationEvents;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Appointments.Events;
using Microsoft.Extensions.Logging;

namespace AgendeAqui.Application.Appointments.Events;

public sealed class AppointmentCancelledDomainEventHandler(
    IEventBus eventBus,
    ITenantProvider tenantProvider,
    IAppointmentHubNotifier hubNotifier,
    ILogger<AppointmentCancelledDomainEventHandler> logger) : IDomainEventHandler<AppointmentCancelledEvent>
{
    public async ValueTask Handle(AppointmentCancelledEvent notification, CancellationToken cancellationToken)
    {
        var tenantId = tenantProvider.GetTenantId();

        logger.LogInformation("Publishing integration event for appointment cancelled: {AppointmentId}", notification.AppointmentId);

        await eventBus.PublishAsync(new AppointmentCancelledIntegrationEvent(
            Guid.NewGuid(),
            DateTime.UtcNow,
            tenantId,
            notification.AppointmentId,
            notification.Reason), cancellationToken);

        await hubNotifier.NotifyAppointmentCancelledAsync(tenantId, notification.AppointmentId, notification.Reason, cancellationToken);
    }
}
