using AgendeAqui.Application.Abstractions.Messaging;

namespace AgendeAqui.Application.ApiKeys.RevokeApiKey;

public sealed record RevokeApiKeyCommand(Guid ApiKeyId) : ICommand;
