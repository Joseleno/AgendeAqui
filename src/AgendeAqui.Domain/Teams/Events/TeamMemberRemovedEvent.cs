using AgendeAqui.Domain.Common;

namespace AgendeAqui.Domain.Teams.Events;

public sealed record TeamMemberRemovedEvent(Guid TeamId, Guid ProfessionalId) : IDomainEvent;
