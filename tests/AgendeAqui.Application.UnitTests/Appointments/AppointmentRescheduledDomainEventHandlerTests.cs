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

public class AppointmentRescheduledDomainEventHandlerTests
{
    private readonly IEventBus _eventBus;
    private readonly ITenantProvider _tenantProvider;
    private readonly IAppointmentHubNotifier _hubNotifier;
    private readonly AppointmentRescheduledDomainEventHandler _handler;

    public AppointmentRescheduledDomainEventHandlerTests()
    {
        _eventBus = Substitute.For<IEventBus>();
        _tenantProvider = Substitute.For<ITenantProvider>();
        _hubNotifier = Substitute.For<IAppointmentHubNotifier>();
        var logger = Substitute.For<ILogger<AppointmentRescheduledDomainEventHandler>>();
        _handler = new AppointmentRescheduledDomainEventHandler(_eventBus, _tenantProvider, _hubNotifier, logger);
    }

    [Fact]
    public async Task Handle_ShouldPublishIntegrationEventWithTenantIdAndNewDate()
    {
        var tenantId = Guid.NewGuid();
        _tenantProvider.GetTenantId().Returns(tenantId);
        var newDate = DateOnly.FromDateTime(DateTime.Today.AddDays(5));
        var domainEvent = new AppointmentRescheduledEvent(Guid.NewGuid(), newDate);

        await _handler.Handle(domainEvent, CancellationToken.None);

        await _eventBus.Received(1).PublishAsync(
            Arg.Is<AppointmentRescheduledIntegrationEvent>(e =>
                e.AppointmentId == domainEvent.AppointmentId &&
                e.NewDate == newDate &&
                e.TenantId == tenantId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldNotifyViaSignalR()
    {
        var tenantId = Guid.NewGuid();
        _tenantProvider.GetTenantId().Returns(tenantId);
        var newDate = DateOnly.FromDateTime(DateTime.Today.AddDays(5));
        var domainEvent = new AppointmentRescheduledEvent(Guid.NewGuid(), newDate);

        await _handler.Handle(domainEvent, CancellationToken.None);

        await _hubNotifier.Received(1).NotifyAppointmentRescheduledAsync(
            tenantId,
            domainEvent.AppointmentId,
            domainEvent.NewDate,
            Arg.Any<CancellationToken>());
    }
}
