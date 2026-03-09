using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Application.Abstractions.RealTime;
using AgendeAqui.Application.Appointments.Events;
using AgendeAqui.Application.Appointments.IntegrationEvents;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Appointments.Events;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace AgendeAqui.Application.UnitTests.Appointments;

public class AppointmentCreatedDomainEventHandlerTests
{
    private readonly IEventBus _eventBus;
    private readonly ITenantProvider _tenantProvider;
    private readonly IAppointmentHubNotifier _hubNotifier;
    private readonly AppointmentCreatedDomainEventHandler _handler;
    private readonly Guid _tenantId = Guid.NewGuid();

    public AppointmentCreatedDomainEventHandlerTests()
    {
        _eventBus = Substitute.For<IEventBus>();
        _tenantProvider = Substitute.For<ITenantProvider>();
        _tenantProvider.GetTenantId().Returns(_tenantId);
        _hubNotifier = Substitute.For<IAppointmentHubNotifier>();
        var logger = Substitute.For<ILogger<AppointmentCreatedDomainEventHandler>>();
        _handler = new AppointmentCreatedDomainEventHandler(_eventBus, _tenantProvider, _hubNotifier, logger);
    }

    [Fact]
    public async Task Handle_ShouldPublishIntegrationEventWithTenantId()
    {
        var domainEvent = new AppointmentCreatedEvent(Guid.NewGuid());

        await _handler.Handle(domainEvent, CancellationToken.None);

        await _eventBus.Received(1).PublishAsync(
            Arg.Is<AppointmentCreatedIntegrationEvent>(e =>
                e.AppointmentId == domainEvent.AppointmentId &&
                e.TenantId == _tenantId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldNotifyViaSignalR()
    {
        var domainEvent = new AppointmentCreatedEvent(Guid.NewGuid());

        await _handler.Handle(domainEvent, CancellationToken.None);

        await _hubNotifier.Received(1).NotifyAppointmentCreatedAsync(
            _tenantId,
            domainEvent.AppointmentId,
            Arg.Any<CancellationToken>());
    }
}
