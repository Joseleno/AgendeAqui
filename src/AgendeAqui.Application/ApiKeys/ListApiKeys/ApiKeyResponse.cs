namespace AgendeAqui.Application.ApiKeys.ListApiKeys;

public sealed record ApiKeyResponse(
    Guid Id,
    string Name,
    string KeyPrefix,
    bool IsActive,
    DateTime? ExpiresAt,
    DateTime CreatedAt);
