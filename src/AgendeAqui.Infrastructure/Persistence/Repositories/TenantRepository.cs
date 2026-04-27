using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Tenants;
using Microsoft.EntityFrameworkCore;

namespace AgendeAqui.Infrastructure.Persistence.Repositories;

internal sealed class TenantRepository : ITenantRepository
{
    private readonly ApplicationDbContext _context;

    public TenantRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Tenant?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _context.Tenants.FindAsync([id], ct);

    public async Task<IReadOnlyList<Tenant>> GetAllAsync(CancellationToken ct = default) =>
        await _context.Tenants.AsNoTracking().ToListAsync(ct);

    public async Task<Tenant?> GetBySlugAsync(string slug, CancellationToken ct = default) =>
        await _context.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Slug == slug, ct);

    public async Task<Tenant?> GetByCustomDomainAsync(string domain, CancellationToken ct = default) =>
        await _context.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.CustomDomain == domain, ct);

    public async Task AddAsync(Tenant entity, CancellationToken ct = default) =>
        await _context.Tenants.AddAsync(entity, ct);

    public void Update(Tenant entity) =>
        _context.Tenants.Update(entity);

    public void Remove(Tenant entity) =>
        _context.Tenants.Remove(entity);
}
