using AgendeAqui.Domain.ApiKeys;

namespace AgendeAqui.Domain.Abstractions;

public interface IApiKeyRepository
{
    Task<ApiKey?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(ApiKey apiKey, CancellationToken ct = default);
}
