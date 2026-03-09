using AgendeAqui.Application.Webhooks.IntegrationEvents;
using AgendeAqui.Infrastructure.Webhooks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AgendeAqui.Infrastructure.Messaging.Consumers;

internal sealed class WebhookDeliveryConsumer : RabbitMqConsumerBase<WebhookDeliveryIntegrationEvent>
{
    public WebhookDeliveryConsumer(
        RabbitMqConnection connection,
        IServiceScopeFactory scopeFactory,
        ILogger<WebhookDeliveryConsumer> logger)
        : base(connection, scopeFactory, logger, "integrations.webhook") { }

    protected override async Task ProcessAsync(
        WebhookDeliveryIntegrationEvent message,
        IServiceScope scope,
        CancellationToken ct)
    {
        var dispatcher = scope.ServiceProvider.GetRequiredService<WebhookDispatcher>();
        await dispatcher.DispatchAsync(message.EventType, message.Payload, ct);
    }
}
