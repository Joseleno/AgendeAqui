using System.Text.Json;
using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Application.Webhooks.IntegrationEvents;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Appointments.Events;

namespace AgendeAqui.Application.Webhooks.Events;

public sealed class AppointmentRescheduledWebhookHandler(
    IEventBus eventBus,
    ITenantProvider tenantProvider) : IDomainEventHandler<AppointmentRescheduledEvent>
{
    public async ValueTask Handle(AppointmentRescheduledEvent notification, CancellationToken cancellationToken)
    {
        var payload = JsonSerializer.Serialize(new
        {
            event_type = "appointment.rescheduled",
            tenant_id = tenantProvider.GetTenantId(),
            appointment_id = notification.AppointmentId,
            new_date = notification.NewDate.ToString("yyyy-MM-dd"),
            source_api_key_id = notification.SourceApiKeyId,
            occurred_at = DateTime.UtcNow
        });

        await eventBus.PublishAsync(new WebhookDeliveryIntegrationEvent(
            Guid.NewGuid(),
            DateTime.UtcNow,
            WebhookId: Guid.Empty,
            EventType: "appointment.rescheduled",
            Payload: payload,
            SourceApiKeyId: notification.SourceApiKeyId), cancellationToken);
    }
}
