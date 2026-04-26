using AgendeAqui.Domain.Common;

namespace AgendeAqui.Domain.Teams.Events;

public sealed record TeamDisbandedEvent(Guid TeamId, Guid TenantId) : IDomainEvent;
