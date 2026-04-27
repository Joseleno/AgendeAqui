using AgendeAqui.Domain.Tenants;

namespace AgendeAqui.Domain.Abstractions;

public interface ITenantRepository : IRepository<Tenant>
{
    Task<Tenant?> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task<Tenant?> GetByCustomDomainAsync(string domain, CancellationToken ct = default);
}
