using AgendeAqui.Domain.Common;

namespace AgendeAqui.Domain.Webhooks;

public sealed class WebhookDelivery : TenantEntity
{
    public Guid WebhookId { get; private set; }
    public string EventType { get; private set; } = default!;
    public string Payload { get; private set; } = default!;
    public int? ResponseCode { get; private set; }
    public int Attempt { get; private set; }

    private WebhookDelivery() { }

    public static Result<WebhookDelivery> Create(Guid tenantId, Guid webhookId, string eventType, string payload)
    {
        if (tenantId == Guid.Empty)
            return Result.Failure<WebhookDelivery>(WebhookErrors.InvalidTenant);

        if (webhookId == Guid.Empty)
            return Result.Failure<WebhookDelivery>(WebhookErrors.InvalidWebhookId);

        if (string.IsNullOrWhiteSpace(eventType))
            return Result.Failure<WebhookDelivery>(WebhookErrors.InvalidEvents);

        if (string.IsNullOrWhiteSpace(payload))
            return Result.Failure<WebhookDelivery>(WebhookErrors.InvalidPayload);

        if (payload.Length > MaxPayloadLength)
            return Result.Failure<WebhookDelivery>(WebhookErrors.PayloadTooLarge);

        var delivery = new WebhookDelivery
        {
            TenantId = tenantId,
            WebhookId = webhookId,
            EventType = eventType,
            Payload = payload,
            Attempt = 1
        };

        return Result.Success(delivery);
    }

    public void RecordResult(int responseCode)
    {
        ResponseCode = responseCode;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsSuccess => ResponseCode.HasValue && ResponseCode.Value is >= 200 and < 300;

    public const int MaxPayloadLength = 65_536; // 64 KB

    public const int MaxAttempts = 5;

    public bool HasExceededMaxAttempts => Attempt >= MaxAttempts;

    public void RecordFailure()
    {
        Attempt++;
        UpdatedAt = DateTime.UtcNow;
    }
}
