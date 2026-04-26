using AgendeAqui.Domain.Common;

namespace AgendeAqui.Domain.Teams.Events;

public sealed record TeamLeaderChangedEvent(Guid TeamId, Guid? NewLeaderId) : IDomainEvent;
