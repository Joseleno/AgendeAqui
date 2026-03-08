using AgendeAqui.Domain.Common;

namespace AgendeAqui.Domain.Clients.Events;

public sealed record DataDeletionRequestedEvent(Guid ClientId, Guid TenantId) : IDomainEvent;
