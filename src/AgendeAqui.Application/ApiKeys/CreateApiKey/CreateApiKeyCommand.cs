using AgendeAqui.Application.Abstractions.Messaging;

namespace AgendeAqui.Application.ApiKeys.CreateApiKey;

public sealed record CreateApiKeyCommand(string Name, DateTime? ExpiresAt) : ICommand<CreateApiKeyResponse>;
