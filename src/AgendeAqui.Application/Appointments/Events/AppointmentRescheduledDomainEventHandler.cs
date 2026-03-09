using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Application.Appointments.IntegrationEvents;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Appointments.Events;
using Microsoft.Extensions.Logging;

namespace AgendeAqui.Application.Appointments.Events;

public sealed class AppointmentRescheduledDomainEventHandler(
    IEventBus eventBus,
    ITenantProvider tenantProvider,
    ILogger<AppointmentRescheduledDomainEventHandler> logger) : IDomainEventHandler<AppointmentRescheduledEvent>
{
    public async ValueTask Handle(AppointmentRescheduledEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Publishing integration event for appointment rescheduled: {AppointmentId}", notification.AppointmentId);

        await eventBus.PublishAsync(new AppointmentRescheduledIntegrationEvent(
            Guid.NewGuid(),
            DateTime.UtcNow,
            tenantProvider.GetTenantId(),
            notification.AppointmentId,
            notification.NewDate), cancellationToken);
    }
}
