using AgendeAqui.Application.Abstractions.Messaging;

namespace AgendeAqui.Application.Webhooks.ListWebhooks;

public sealed record ListWebhooksQuery() : IQuery<List<WebhookResponse>>;
