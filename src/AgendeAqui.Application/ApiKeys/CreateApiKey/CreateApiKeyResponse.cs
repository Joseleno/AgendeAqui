namespace AgendeAqui.Application.ApiKeys.CreateApiKey;

public sealed record CreateApiKeyResponse(Guid Id, string RawKey, string Name, DateTime? ExpiresAt);
