using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Application.Abstractions.RealTime;
using AgendeAqui.Application.Appointments.IntegrationEvents;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Appointments.Events;
using Microsoft.Extensions.Logging;

namespace AgendeAqui.Application.Appointments.Events;

public sealed class AppointmentCreatedDomainEventHandler(
    IEventBus eventBus,
    ITenantProvider tenantProvider,
    IAppointmentHubNotifier hubNotifier,
    ILogger<AppointmentCreatedDomainEventHandler> logger) : IDomainEventHandler<AppointmentCreatedEvent>
{
    public async ValueTask Handle(AppointmentCreatedEvent notification, CancellationToken cancellationToken)
    {
        var tenantId = tenantProvider.GetTenantId();

        logger.LogInformation("Publishing integration event for appointment created: {AppointmentId}", notification.AppointmentId);

        await eventBus.PublishAsync(new AppointmentCreatedIntegrationEvent(
            Guid.NewGuid(),
            DateTime.UtcNow,
            tenantId,
            notification.AppointmentId), cancellationToken);

        await hubNotifier.NotifyAppointmentCreatedAsync(tenantId, notification.AppointmentId, cancellationToken);
    }
}
