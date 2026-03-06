using AgendeAqui.Domain.Professionals;

namespace AgendeAqui.Domain.Abstractions;

public interface IProfessionalRepository : IRepository<Professional>
{
    Task<IReadOnlyList<Professional>> GetActiveByTenantAsync(Guid tenantId, CancellationToken ct = default);
}
