using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace AgendeAqui.Infrastructure.Messaging;

internal sealed class RabbitMqSetup : IHostedService
{
    private readonly RabbitMqConnection _connection;
    private readonly ILogger<RabbitMqSetup> _logger;

    public RabbitMqSetup(RabbitMqConnection connection, ILogger<RabbitMqSetup> logger)
    {
        _connection = connection;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Setting up RabbitMQ exchanges and queues...");

        await using var channel = await _connection.CreateChannelAsync(cancellationToken);

        // Declare dead-letter exchange first
        await channel.ExchangeDeclareAsync(
            exchange: "scheduling.dlx",
            type: ExchangeType.Fanout,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);

        // Declare main exchanges
        await channel.ExchangeDeclareAsync(
            exchange: "appointment.events",
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);

        await channel.ExchangeDeclareAsync(
            exchange: "notification.events",
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);

        // Dead-letter queue
        await channel.QueueDeclareAsync(
            queue: "scheduling.dlq",
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: cancellationToken);

        await channel.QueueBindAsync(
            queue: "scheduling.dlq",
            exchange: "scheduling.dlx",
            routingKey: string.Empty,
            cancellationToken: cancellationToken);

        // Appointment queues
        var dlxArgs = new Dictionary<string, object?>
        {
            ["x-dead-letter-exchange"] = "scheduling.dlx"
        };

        await channel.QueueDeclareAsync(
            queue: "appointment.created",
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: dlxArgs,
            cancellationToken: cancellationToken);

        await channel.QueueBindAsync(
            queue: "appointment.created",
            exchange: "appointment.events",
            routingKey: "appointmentcreated",
            cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(
            queue: "appointment.cancelled",
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: dlxArgs,
            cancellationToken: cancellationToken);

        await channel.QueueBindAsync(
            queue: "appointment.cancelled",
            exchange: "appointment.events",
            routingKey: "appointmentcancelled",
            cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(
            queue: "appointment.rescheduled",
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: dlxArgs,
            cancellationToken: cancellationToken);

        await channel.QueueBindAsync(
            queue: "appointment.rescheduled",
            exchange: "appointment.events",
            routingKey: "appointmentrescheduled",
            cancellationToken: cancellationToken);

        // Retry queues with TTL
        await channel.QueueDeclareAsync(
            queue: "appointment.retry.1s",
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: new Dictionary<string, object?>
            {
                ["x-dead-letter-exchange"] = "appointment.events",
                ["x-message-ttl"] = 1000
            },
            cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(
            queue: "appointment.retry.5s",
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: new Dictionary<string, object?>
            {
                ["x-dead-letter-exchange"] = "appointment.events",
                ["x-message-ttl"] = 5000
            },
            cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(
            queue: "appointment.retry.25s",
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: new Dictionary<string, object?>
            {
                ["x-dead-letter-exchange"] = "appointment.events",
                ["x-message-ttl"] = 25000
            },
            cancellationToken: cancellationToken);

        // Notification queue
        await channel.QueueDeclareAsync(
            queue: "notifications.whatsapp",
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: dlxArgs,
            cancellationToken: cancellationToken);

        await channel.QueueBindAsync(
            queue: "notifications.whatsapp",
            exchange: "notification.events",
            routingKey: "whatsapp",
            cancellationToken: cancellationToken);

        // Integration events exchange and webhook delivery queue
        await channel.ExchangeDeclareAsync(
            exchange: "integration.events",
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(
            queue: "integrations.webhook",
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: dlxArgs,
            cancellationToken: cancellationToken);

        await channel.QueueBindAsync(
            queue: "integrations.webhook",
            exchange: "integration.events",
            routingKey: "webhookdelivery",
            cancellationToken: cancellationToken);

        _logger.LogInformation("RabbitMQ setup completed.");
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
