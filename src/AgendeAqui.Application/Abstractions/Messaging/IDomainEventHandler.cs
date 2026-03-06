using AgendeAqui.Domain.Common;
using Mediator;

namespace AgendeAqui.Application.Abstractions.Messaging;

public interface IDomainEventHandler<in TEvent> : INotificationHandler<TEvent>
    where TEvent : IDomainEvent;
