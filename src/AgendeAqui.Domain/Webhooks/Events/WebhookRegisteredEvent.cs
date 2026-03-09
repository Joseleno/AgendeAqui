using AgendeAqui.Domain.Common;

namespace AgendeAqui.Domain.Webhooks.Events;

public sealed record WebhookRegisteredEvent(Guid WebhookId) : IDomainEvent;
