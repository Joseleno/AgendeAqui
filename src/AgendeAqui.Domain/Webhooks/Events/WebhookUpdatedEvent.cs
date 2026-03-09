using AgendeAqui.Domain.Common;

namespace AgendeAqui.Domain.Webhooks.Events;

public sealed record WebhookUpdatedEvent(Guid WebhookId) : IDomainEvent;
