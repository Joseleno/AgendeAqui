using AgendeAqui.Application.Abstractions.Messaging;

namespace AgendeAqui.Application.Webhooks.IntegrationEvents;

public sealed record WebhookDeliveryIntegrationEvent(
    Guid Id,
    DateTime OccurredAt,
    Guid WebhookId,
    string EventType,
    string Payload) : IntegrationEvent(Id, OccurredAt);
