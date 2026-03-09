using AgendeAqui.Application.Abstractions.Messaging;

namespace AgendeAqui.Application.Webhooks.DeleteWebhook;

public sealed record DeleteWebhookCommand(Guid WebhookId) : ICommand;
