using AgendeAqui.Application.Abstractions.Messaging;

namespace AgendeAqui.Application.Webhooks.UpdateWebhook;

public sealed record UpdateWebhookCommand(
    Guid WebhookId,
    string Url,
    IReadOnlyList<string> Events,
    bool IsActive) : ICommand;
