using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Application.Appointments.IntegrationEvents;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Appointments.Events;
using Microsoft.Extensions.Logging;

namespace AgendeAqui.Application.Appointments.Events;

public sealed class AppointmentCreatedDomainEventHandler(
    IEventBus eventBus,
    ITenantProvider tenantProvider,
    ILogger<AppointmentCreatedDomainEventHandler> logger) : IDomainEventHandler<AppointmentCreatedEvent>
{
    public async ValueTask Handle(AppointmentCreatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Publishing integration event for appointment created: {AppointmentId}", notification.AppointmentId);

        await eventBus.PublishAsync(new AppointmentCreatedIntegrationEvent(
            Guid.NewGuid(),
            DateTime.UtcNow,
            tenantProvider.GetTenantId(),
            notification.AppointmentId), cancellationToken);
    }
}
