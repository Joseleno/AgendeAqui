using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

namespace AgendeAqui.Infrastructure.Messaging;

internal sealed class RabbitMqEventBus : IEventBus
{
    private const string AppointmentEventsExchange = "appointment.events";

    private readonly RabbitMqConnection _connection;

    public RabbitMqEventBus(RabbitMqConnection connection)
    {
        _connection = connection;
    }

    public async Task PublishAsync<T>(T @event, CancellationToken ct = default) where T : IntegrationEvent
    {
        await using var channel = await _connection.CreateChannelAsync(ct);

        var json = JsonSerializer.Serialize(@event, @event.GetType());
        var body = Encoding.UTF8.GetBytes(json);

        var routingKey = typeof(T).Name.ToLowerInvariant();

        var properties = new BasicProperties
        {
            ContentType = "application/json",
            DeliveryMode = DeliveryModes.Persistent,
            MessageId = @event.Id.ToString(),
            Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds())
        };

        await channel.BasicPublishAsync(
            exchange: AppointmentEventsExchange,
            routingKey: routingKey,
            mandatory: false,
            basicProperties: properties,
            body: body,
            cancellationToken: ct);
    }
}
