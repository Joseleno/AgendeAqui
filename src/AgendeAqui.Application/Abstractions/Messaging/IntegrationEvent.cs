namespace AgendeAqui.Application.Abstractions.Messaging;

public abstract record IntegrationEvent(Guid Id, DateTime OccurredAt);
