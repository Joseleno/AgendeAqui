using AgendeAqui.Domain.Common;

namespace AgendeAqui.Domain.Teams.Events;

public sealed record TeamMemberAddedEvent(Guid TeamId, Guid ProfessionalId) : IDomainEvent;
