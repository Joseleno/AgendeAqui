using AgendeAqui.Domain.Common;

namespace AgendeAqui.Domain.ApiKeys;

public sealed class ApiKey : TenantEntity
{
    public string Name { get; private set; } = default!;
    public string KeyHash { get; private set; } = default!;
    public bool IsActive { get; private set; }
    public DateTime? ExpiresAt { get; private set; }

    private ApiKey() { }

    public static Result<ApiKey> Create(Guid tenantId, string name, string keyHash, DateTime? expiresAt = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<ApiKey>(ApiKeyErrors.EmptyName);

        if (string.IsNullOrWhiteSpace(keyHash))
            return Result.Failure<ApiKey>(ApiKeyErrors.EmptyKeyHash);

        return Result.Success(new ApiKey
        {
            TenantId = tenantId,
            Name = name,
            KeyHash = keyHash,
            IsActive = true,
            ExpiresAt = expiresAt
        });
    }

    public void Revoke()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}
