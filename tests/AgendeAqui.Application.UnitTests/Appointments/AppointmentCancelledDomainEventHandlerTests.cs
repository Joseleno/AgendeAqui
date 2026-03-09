using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Application.Appointments.Events;
using AgendeAqui.Application.Appointments.IntegrationEvents;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Appointments.Events;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace AgendeAqui.Application.UnitTests.Appointments;

public class AppointmentCancelledDomainEventHandlerTests
{
    private readonly IEventBus _eventBus;
    private readonly ITenantProvider _tenantProvider;
    private readonly AppointmentCancelledDomainEventHandler _handler;

    public AppointmentCancelledDomainEventHandlerTests()
    {
        _eventBus = Substitute.For<IEventBus>();
        _tenantProvider = Substitute.For<ITenantProvider>();
        var logger = Substitute.For<ILogger<AppointmentCancelledDomainEventHandler>>();
        _handler = new AppointmentCancelledDomainEventHandler(_eventBus, _tenantProvider, logger);
    }

    [Fact]
    public async Task Handle_ShouldPublishIntegrationEventWithTenantIdAndReason()
    {
        var tenantId = Guid.NewGuid();
        _tenantProvider.GetTenantId().Returns(tenantId);
        var domainEvent = new AppointmentCancelledEvent(Guid.NewGuid(), "Patient request");

        await _handler.Handle(domainEvent, CancellationToken.None);

        await _eventBus.Received(1).PublishAsync(
            Arg.Is<AppointmentCancelledIntegrationEvent>(e =>
                e.AppointmentId == domainEvent.AppointmentId &&
                e.Reason == domainEvent.Reason &&
                e.TenantId == tenantId),
            Arg.Any<CancellationToken>());
    }
}
