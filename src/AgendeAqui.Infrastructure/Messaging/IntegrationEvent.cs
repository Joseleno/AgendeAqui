namespace AgendeAqui.Infrastructure.Messaging;

public abstract record IntegrationEvent(Guid Id, DateTime OccurredAt);
