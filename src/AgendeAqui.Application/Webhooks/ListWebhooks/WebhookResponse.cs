using System.Text.Json;

namespace AgendeAqui.Application.Webhooks.ListWebhooks;

public sealed record WebhookResponse(
    Guid Id,
    string Url,
    List<string> Events,
    bool IsActive,
    DateTime CreatedAt);

internal sealed record WebhookRow(
    Guid Id,
    string Url,
    string Events,
    bool IsActive,
    DateTime CreatedAt)
{
    public WebhookResponse ToResponse() => new(
        Id, Url,
        JsonSerializer.Deserialize<List<string>>(Events, JsonSerializerOptions.Default) ?? [],
        IsActive, CreatedAt);
}
