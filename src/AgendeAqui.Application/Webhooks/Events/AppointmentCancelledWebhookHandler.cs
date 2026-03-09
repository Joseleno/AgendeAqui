using System.Text.Json;
using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Application.Webhooks.IntegrationEvents;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Appointments.Events;

namespace AgendeAqui.Application.Webhooks.Events;

public sealed class AppointmentCancelledWebhookHandler(
    IEventBus eventBus,
    ITenantProvider tenantProvider) : IDomainEventHandler<AppointmentCancelledEvent>
{
    public async ValueTask Handle(AppointmentCancelledEvent notification, CancellationToken cancellationToken)
    {
        var payload = JsonSerializer.Serialize(new
        {
            event_type = "appointment.cancelled",
            tenant_id = tenantProvider.GetTenantId(),
            appointment_id = notification.AppointmentId,
            reason = notification.Reason,
            occurred_at = DateTime.UtcNow
        });

        await eventBus.PublishAsync(new WebhookDeliveryIntegrationEvent(
            Guid.NewGuid(),
            DateTime.UtcNow,
            WebhookId: Guid.Empty,
            EventType: "appointment.cancelled",
            Payload: payload), cancellationToken);
    }
}
