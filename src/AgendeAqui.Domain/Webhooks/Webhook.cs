using AgendeAqui.Domain.Common;

namespace AgendeAqui.Domain.Webhooks;

public sealed class Webhook : TenantEntity
{
    public string Url { get; private set; } = default!;
    public string SecretHash { get; private set; } = default!;
    public IReadOnlyList<string> Events { get; private set; } = [];
    public bool IsActive { get; private set; }

    private Webhook() { }

    public static Result<Webhook> Create(Guid tenantId, string url, string secretHash, IReadOnlyList<string> events)
    {
        if (tenantId == Guid.Empty)
            return Result.Failure<Webhook>(WebhookErrors.InvalidTenant);

        if (string.IsNullOrWhiteSpace(url))
            return Result.Failure<Webhook>(WebhookErrors.InvalidUrl);

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            return Result.Failure<Webhook>(WebhookErrors.InvalidUrl);

        if (url.Length > 2048)
            return Result.Failure<Webhook>(WebhookErrors.InvalidUrl);

        if (string.IsNullOrWhiteSpace(secretHash))
            return Result.Failure<Webhook>(WebhookErrors.InvalidSecret);

        if (events is null || events.Count == 0)
            return Result.Failure<Webhook>(WebhookErrors.InvalidEvents);

        var webhook = new Webhook
        {
            TenantId = tenantId,
            Url = url,
            SecretHash = secretHash,
            Events = events.ToList(),
            IsActive = true
        };

        return Result.Success(webhook);
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public Result<Webhook> Update(string url, IReadOnlyList<string> events, bool isActive)
    {
        if (string.IsNullOrWhiteSpace(url))
            return Result.Failure<Webhook>(WebhookErrors.InvalidUrl);

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            return Result.Failure<Webhook>(WebhookErrors.InvalidUrl);

        if (url.Length > 2048)
            return Result.Failure<Webhook>(WebhookErrors.InvalidUrl);

        if (events is null || events.Count == 0)
            return Result.Failure<Webhook>(WebhookErrors.InvalidEvents);

        Url = url;
        Events = events.ToList();
        IsActive = isActive;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success(this);
    }
}
