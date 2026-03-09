using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace AgendeAqui.Infrastructure.Messaging.Consumers;

internal abstract class RabbitMqConsumerBase<T> : BackgroundService where T : class
{
    private const int MaxRetries = 3;

    private readonly RabbitMqConnection _connection;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger _logger;
    private readonly string _queueName;

    protected RabbitMqConsumerBase(
        RabbitMqConnection connection,
        IServiceScopeFactory scopeFactory,
        ILogger logger,
        string queueName)
    {
        _connection = connection;
        _scopeFactory = scopeFactory;
        _logger = logger;
        _queueName = queueName;
    }

    protected abstract Task ProcessAsync(T message, IServiceScope scope, CancellationToken ct);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var channel = await _connection.CreateChannelAsync(stoppingToken);
        await channel.BasicQosAsync(0, 1, false, stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (_, ea) =>
        {
            var retryCount = GetRetryCount(ea.BasicProperties);

            try
            {
                var body = Encoding.UTF8.GetString(ea.Body.Span);
                var message = JsonSerializer.Deserialize<T>(body);

                if (message is null)
                {
                    _logger.LogWarning("Failed to deserialize message from {Queue}", _queueName);
                    await channel.BasicNackAsync(ea.DeliveryTag, false, false, stoppingToken);
                    return;
                }

                using var scope = _scopeFactory.CreateScope();
                await ProcessAsync(message, scope, stoppingToken);
                await channel.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);

                _logger.LogInformation("Processed message from {Queue}", _queueName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message from {Queue}, retry {RetryCount}/{MaxRetries}",
                    _queueName, retryCount + 1, MaxRetries);

                if (retryCount >= MaxRetries - 1)
                {
                    _logger.LogWarning("Message from {Queue} exceeded max retries, sending to DLQ", _queueName);
                    await channel.BasicNackAsync(ea.DeliveryTag, false, false, stoppingToken);
                }
                else
                {
                    // Exponential backoff before republishing: 1s, 5s, 25s
                    // Blocks the consumer intentionally (prefetch=1) to avoid hammering a broken dependency
                    var delays = new[] { 1_000, 5_000, 25_000 };
                    var delayMs = delays[Math.Min(retryCount, delays.Length - 1)];

                    _logger.LogWarning(
                        "Retrying message from {Queue} in {DelayMs}ms (attempt {Attempt}/{MaxRetries})",
                        _queueName, delayMs, retryCount + 1, MaxRetries);

                    await Task.Delay(delayMs, stoppingToken);

                    var props = new BasicProperties();
                    props.Headers = new Dictionary<string, object?> { ["x-retry-count"] = retryCount + 1 };
                    await channel.BasicPublishAsync(
                        exchange: string.Empty,
                        routingKey: _queueName,
                        mandatory: false,
                        basicProperties: props,
                        body: ea.Body,
                        cancellationToken: stoppingToken);
                    await channel.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);
                }
            }
        };

        await channel.BasicConsumeAsync(
            queue: _queueName,
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken);

        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            // Graceful shutdown
        }
    }

    private static int GetRetryCount(IReadOnlyBasicProperties properties)
    {
        if (properties.Headers is not null &&
            properties.Headers.TryGetValue("x-retry-count", out var value) &&
            value is int count)
        {
            return count;
        }
        return 0;
    }
}
