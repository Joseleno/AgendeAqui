using AgendeAqui.Domain.Common;
using AgendeAqui.Domain.Webhooks.Events;

namespace AgendeAqui.Domain.Webhooks;

public sealed class Webhook : AggregateRoot
{
    public string Url { get; private set; } = default!;
    public string SecretHash { get; private set; } = default!;
    public IReadOnlyList<string> Events { get; private set; } = [];
    public bool IsActive { get; private set; }
    public bool IsDeleted { get; private set; }

    private Webhook() { }

    public static Result<Webhook> Create(Guid tenantId, string url, string secretHash, IReadOnlyList<string> events)
    {
        if (tenantId == Guid.Empty)
            return Result.Failure<Webhook>(WebhookErrors.InvalidTenant);

        var urlValidation = ValidateUrl(url);
        if (urlValidation.IsFailure)
            return Result.Failure<Webhook>(urlValidation.Error);

        if (string.IsNullOrWhiteSpace(secretHash))
            return Result.Failure<Webhook>(WebhookErrors.InvalidSecret);

        var eventsValidation = ValidateEvents(events);
        if (eventsValidation.IsFailure)
            return Result.Failure<Webhook>(eventsValidation.Error);

        var webhook = new Webhook
        {
            TenantId = tenantId,
            Url = url,
            SecretHash = secretHash,
            Events = events.ToList(),
            IsActive = true
        };

        webhook.RaiseDomainEvent(new WebhookRegisteredEvent(webhook.Id));

        return Result.Success(webhook);
    }

    public void Activate()
    {
        if (IsActive) return;

        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
        RaiseDomainEvent(new WebhookUpdatedEvent(Id));
    }

    public void Deactivate()
    {
        if (!IsActive) return;

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
        RaiseDomainEvent(new WebhookUpdatedEvent(Id));
    }

    public Result<Webhook> Update(string url, IReadOnlyList<string> events, bool isActive)
    {
        var urlValidation = ValidateUrl(url);
        if (urlValidation.IsFailure)
            return Result.Failure<Webhook>(urlValidation.Error);

        var eventsValidation = ValidateEvents(events);
        if (eventsValidation.IsFailure)
            return Result.Failure<Webhook>(eventsValidation.Error);

        Url = url;
        Events = events.ToList();
        IsActive = isActive;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new WebhookUpdatedEvent(Id));

        return Result.Success(this);
    }

    public void Delete()
    {
        if (IsDeleted) return;

        IsDeleted = true;
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
        RaiseDomainEvent(new WebhookDeletedEvent(Id));
    }

    private static Result ValidateUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return Result.Failure(WebhookErrors.InvalidUrl);

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            return Result.Failure(WebhookErrors.InvalidUrl);

        if (url.Length > 2048)
            return Result.Failure(WebhookErrors.InvalidUrl);

        return Result.Success();
    }

    private static Result ValidateEvents(IReadOnlyList<string>? events)
    {
        if (events is null || events.Count == 0)
            return Result.Failure(WebhookErrors.InvalidEvents);

        if (events.Any(e => string.IsNullOrWhiteSpace(e)))
            return Result.Failure(WebhookErrors.InvalidEvents);

        return Result.Success();
    }
}
