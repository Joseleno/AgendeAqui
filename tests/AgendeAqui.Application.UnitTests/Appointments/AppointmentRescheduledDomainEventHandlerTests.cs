using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Application.Appointments.Events;
using AgendeAqui.Application.Appointments.IntegrationEvents;
using AgendeAqui.Domain.Appointments.Events;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace AgendeAqui.Application.UnitTests.Appointments;

public class AppointmentRescheduledDomainEventHandlerTests
{
    private readonly IEventBus _eventBus;
    private readonly AppointmentRescheduledDomainEventHandler _handler;

    public AppointmentRescheduledDomainEventHandlerTests()
    {
        _eventBus = Substitute.For<IEventBus>();
        var logger = Substitute.For<ILogger<AppointmentRescheduledDomainEventHandler>>();
        _handler = new AppointmentRescheduledDomainEventHandler(_eventBus, logger);
    }

    [Fact]
    public async Task Handle_ShouldPublishIntegrationEventWithNewDate()
    {
        var newDate = DateOnly.FromDateTime(DateTime.Today.AddDays(5));
        var domainEvent = new AppointmentRescheduledEvent(Guid.NewGuid(), newDate);

        await _handler.Handle(domainEvent, CancellationToken.None);

        await _eventBus.Received(1).PublishAsync(
            Arg.Is<AppointmentRescheduledIntegrationEvent>(e =>
                e.AppointmentId == domainEvent.AppointmentId &&
                e.NewDate == newDate),
            Arg.Any<CancellationToken>());
    }
}
