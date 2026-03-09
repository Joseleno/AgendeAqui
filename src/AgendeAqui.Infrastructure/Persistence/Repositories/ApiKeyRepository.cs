using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.ApiKeys;
using Microsoft.EntityFrameworkCore;

namespace AgendeAqui.Infrastructure.Persistence.Repositories;

internal sealed class ApiKeyRepository(ApplicationDbContext dbContext) : IApiKeyRepository
{
    public async Task<ApiKey?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await dbContext.Set<ApiKey>().FirstOrDefaultAsync(a => a.Id == id, ct);

    public async Task AddAsync(ApiKey apiKey, CancellationToken ct = default)
        => await dbContext.Set<ApiKey>().AddAsync(apiKey, ct);
}
