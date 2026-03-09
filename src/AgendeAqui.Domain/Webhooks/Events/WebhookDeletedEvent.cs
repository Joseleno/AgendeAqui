using AgendeAqui.Domain.Common;

namespace AgendeAqui.Domain.Webhooks.Events;

public sealed record WebhookDeletedEvent(Guid WebhookId) : IDomainEvent;
