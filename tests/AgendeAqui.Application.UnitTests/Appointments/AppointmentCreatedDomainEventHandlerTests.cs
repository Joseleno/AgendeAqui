using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Application.Appointments.Events;
using AgendeAqui.Application.Appointments.IntegrationEvents;
using AgendeAqui.Domain.Appointments.Events;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace AgendeAqui.Application.UnitTests.Appointments;

public class AppointmentCreatedDomainEventHandlerTests
{
    private readonly IEventBus _eventBus;
    private readonly AppointmentCreatedDomainEventHandler _handler;

    public AppointmentCreatedDomainEventHandlerTests()
    {
        _eventBus = Substitute.For<IEventBus>();
        var logger = Substitute.For<ILogger<AppointmentCreatedDomainEventHandler>>();
        _handler = new AppointmentCreatedDomainEventHandler(_eventBus, logger);
    }

    [Fact]
    public async Task Handle_ShouldPublishIntegrationEvent()
    {
        var domainEvent = new AppointmentCreatedEvent(Guid.NewGuid());

        await _handler.Handle(domainEvent, CancellationToken.None);

        await _eventBus.Received(1).PublishAsync(
            Arg.Is<AppointmentCreatedIntegrationEvent>(e => e.AppointmentId == domainEvent.AppointmentId),
            Arg.Any<CancellationToken>());
    }
}
