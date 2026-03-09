using AgendeAqui.Application.Abstractions.Messaging;

namespace AgendeAqui.Application.Webhooks.RegisterWebhook;

public sealed record RegisterWebhookCommand(string Url, string Secret, IReadOnlyList<string> Events) : ICommand<Guid>;
