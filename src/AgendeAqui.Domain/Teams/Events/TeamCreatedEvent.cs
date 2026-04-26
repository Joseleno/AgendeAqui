using AgendeAqui.Domain.Common;

namespace AgendeAqui.Domain.Teams.Events;

public sealed record TeamCreatedEvent(Guid TeamId, Guid TenantId) : IDomainEvent;
