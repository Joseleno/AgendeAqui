using AgendeAqui.Domain.Services;

namespace AgendeAqui.Domain.Abstractions;

public interface IServiceRepository : IRepository<Service>
{
    Task<IReadOnlyList<Service>> GetActiveByTenantAsync(Guid tenantId, CancellationToken ct = default);
}
