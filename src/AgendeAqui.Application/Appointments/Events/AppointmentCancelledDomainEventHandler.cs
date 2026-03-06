using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Application.Appointments.IntegrationEvents;
using AgendeAqui.Domain.Appointments.Events;
using Microsoft.Extensions.Logging;

namespace AgendeAqui.Application.Appointments.Events;

public sealed class AppointmentCancelledDomainEventHandler(
    IEventBus eventBus,
    ILogger<AppointmentCancelledDomainEventHandler> logger) : IDomainEventHandler<AppointmentCancelledEvent>
{
    public async ValueTask Handle(AppointmentCancelledEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Publishing integration event for appointment cancelled: {AppointmentId}", notification.AppointmentId);

        await eventBus.PublishAsync(new AppointmentCancelledIntegrationEvent(
            Guid.NewGuid(),
            DateTime.UtcNow,
            notification.AppointmentId,
            notification.Reason), cancellationToken);
    }
}
