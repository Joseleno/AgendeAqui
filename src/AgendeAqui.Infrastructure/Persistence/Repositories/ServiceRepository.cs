using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Services;
using Microsoft.EntityFrameworkCore;

namespace AgendeAqui.Infrastructure.Persistence.Repositories;

internal sealed class ServiceRepository : IServiceRepository
{
    private readonly ApplicationDbContext _context;

    public ServiceRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Service?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _context.Services.FindAsync([id], ct);

    public async Task<IReadOnlyList<Service>> GetAllAsync(CancellationToken ct = default) =>
        await _context.Services.AsNoTracking().ToListAsync(ct);

    public async Task<IReadOnlyList<Service>> GetActiveByTenantAsync(Guid tenantId, CancellationToken ct = default) =>
        await _context.Services
            .AsNoTracking()
            .Where(s => s.TenantId == tenantId && s.IsActive)
            .ToListAsync(ct);

    public async Task AddAsync(Service entity, CancellationToken ct = default) =>
        await _context.Services.AddAsync(entity, ct);

    public void Update(Service entity) =>
        _context.Services.Update(entity);

    public void Remove(Service entity) =>
        _context.Services.Remove(entity);
}
