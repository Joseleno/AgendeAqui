using System.Text.Json;
using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Application.Webhooks.IntegrationEvents;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Appointments.Events;

namespace AgendeAqui.Application.Webhooks.Events;

public sealed class AppointmentCreatedWebhookHandler(
    IEventBus eventBus,
    ITenantProvider tenantProvider) : IDomainEventHandler<AppointmentCreatedEvent>
{
    public async ValueTask Handle(AppointmentCreatedEvent notification, CancellationToken cancellationToken)
    {
        var payload = JsonSerializer.Serialize(new
        {
            event_type = "appointment.created",
            tenant_id = tenantProvider.GetTenantId(),
            appointment_id = notification.AppointmentId,
            occurred_at = DateTime.UtcNow
        });

        await eventBus.PublishAsync(new WebhookDeliveryIntegrationEvent(
            Guid.NewGuid(),
            DateTime.UtcNow,
            WebhookId: Guid.Empty,
            EventType: "appointment.created",
            Payload: payload), cancellationToken);
    }
}
