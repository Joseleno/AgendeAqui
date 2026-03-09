using System.Text.Json;
using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Application.Webhooks.Events;
using AgendeAqui.Application.Webhooks.IntegrationEvents;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Appointments.Events;
using FluentAssertions;
using NSubstitute;

namespace AgendeAqui.Application.UnitTests.Webhooks;

public class WebhookEventHandlerTests
{
    private readonly IEventBus _eventBus = Substitute.For<IEventBus>();
    private readonly ITenantProvider _tenantProvider = Substitute.For<ITenantProvider>();
    private readonly Guid _tenantId = Guid.NewGuid();

    public WebhookEventHandlerTests()
    {
        _tenantProvider.GetTenantId().Returns(_tenantId);
    }

    [Fact]
    public async Task AppointmentCreated_ShouldPublishWebhookDeliveryEvent()
    {
        var handler = new AppointmentCreatedWebhookHandler(_eventBus, _tenantProvider);
        var appointmentId = Guid.NewGuid();
        var domainEvent = new AppointmentCreatedEvent(appointmentId);

        await handler.Handle(domainEvent, CancellationToken.None);

        await _eventBus.Received(1).PublishAsync(
            Arg.Is<WebhookDeliveryIntegrationEvent>(e =>
                e.EventType == "appointment.created" &&
                e.Payload.Contains(appointmentId.ToString())),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AppointmentCreated_PayloadShouldContainTenantId()
    {
        var handler = new AppointmentCreatedWebhookHandler(_eventBus, _tenantProvider);
        var domainEvent = new AppointmentCreatedEvent(Guid.NewGuid());

        await handler.Handle(domainEvent, CancellationToken.None);

        await _eventBus.Received(1).PublishAsync(
            Arg.Is<WebhookDeliveryIntegrationEvent>(e =>
                e.Payload.Contains(_tenantId.ToString())),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AppointmentCancelled_ShouldPublishWebhookDeliveryEvent()
    {
        var handler = new AppointmentCancelledWebhookHandler(_eventBus, _tenantProvider);
        var appointmentId = Guid.NewGuid();
        var domainEvent = new AppointmentCancelledEvent(appointmentId, "Client no-show");

        await handler.Handle(domainEvent, CancellationToken.None);

        await _eventBus.Received(1).PublishAsync(
            Arg.Is<WebhookDeliveryIntegrationEvent>(e =>
                e.EventType == "appointment.cancelled" &&
                e.Payload.Contains(appointmentId.ToString()) &&
                e.Payload.Contains("Client no-show")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AppointmentRescheduled_ShouldPublishWebhookDeliveryEvent()
    {
        var handler = new AppointmentRescheduledWebhookHandler(_eventBus, _tenantProvider);
        var appointmentId = Guid.NewGuid();
        var newDate = new DateOnly(2026, 4, 15);
        var domainEvent = new AppointmentRescheduledEvent(appointmentId, newDate);

        await handler.Handle(domainEvent, CancellationToken.None);

        await _eventBus.Received(1).PublishAsync(
            Arg.Is<WebhookDeliveryIntegrationEvent>(e =>
                e.EventType == "appointment.rescheduled" &&
                e.Payload.Contains(appointmentId.ToString()) &&
                e.Payload.Contains("2026-04-15")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AppointmentRescheduled_PayloadShouldBeValidJson()
    {
        var handler = new AppointmentRescheduledWebhookHandler(_eventBus, _tenantProvider);
        var domainEvent = new AppointmentRescheduledEvent(Guid.NewGuid(), new DateOnly(2026, 5, 1));

        WebhookDeliveryIntegrationEvent? captured = null;
        await _eventBus.PublishAsync(
            Arg.Do<WebhookDeliveryIntegrationEvent>(e => captured = e),
            Arg.Any<CancellationToken>());

        await handler.Handle(domainEvent, CancellationToken.None);

        captured.Should().NotBeNull();
        var act = () => JsonDocument.Parse(captured!.Payload);
        act.Should().NotThrow();
    }
}
