using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Application.Abstractions.RealTime;
using AgendeAqui.Application.Appointments.IntegrationEvents;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Appointments.Events;
using Microsoft.Extensions.Logging;

namespace AgendeAqui.Application.Appointments.Events;

public sealed class AppointmentRescheduledDomainEventHandler(
    IEventBus eventBus,
    ITenantProvider tenantProvider,
    IAppointmentHubNotifier hubNotifier,
    ILogger<AppointmentRescheduledDomainEventHandler> logger) : IDomainEventHandler<AppointmentRescheduledEvent>
{
    public async ValueTask Handle(AppointmentRescheduledEvent notification, CancellationToken cancellationToken)
    {
        var tenantId = tenantProvider.GetTenantId();

        logger.LogInformation("Publishing integration event for appointment rescheduled: {AppointmentId}", notification.AppointmentId);

        await eventBus.PublishAsync(new AppointmentRescheduledIntegrationEvent(
            Guid.NewGuid(),
            DateTime.UtcNow,
            tenantId,
            notification.AppointmentId,
            notification.NewDate), cancellationToken);

        await hubNotifier.NotifyAppointmentRescheduledAsync(tenantId, notification.AppointmentId, notification.NewDate, cancellationToken);
    }
}
