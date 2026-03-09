using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Professionals;
using AgendeAqui.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AgendeAqui.Infrastructure.Persistence.Repositories;

internal sealed class ProfessionalRepository : IProfessionalRepository
{
    private readonly ApplicationDbContext _context;

    public ProfessionalRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Professional?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _context.Professionals.FirstOrDefaultAsync(e => e.Id == id, ct);

    public async Task<IReadOnlyList<Professional>> GetAllAsync(CancellationToken ct = default) =>
        await _context.Professionals.AsNoTracking().ToListAsync(ct);

    public async Task<IReadOnlyList<Professional>> GetActiveByTenantAsync(Guid tenantId, CancellationToken ct = default) =>
        await _context.Professionals
            .AsNoTracking()
            .Where(p => p.TenantId == tenantId && p.IsActive)
            .ToListAsync(ct);

    public async Task AddAsync(Professional entity, CancellationToken ct = default) =>
        await _context.Professionals.AddAsync(entity, ct);

    public void Update(Professional entity) =>
        _context.Professionals.Update(entity);

    public void Remove(Professional entity) =>
        _context.Professionals.Remove(entity);
}
