using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.DataProtection;
using Microsoft.EntityFrameworkCore;

namespace AgendeAqui.Infrastructure.Persistence.Repositories;

internal sealed class DataDeletionRequestRepository : IDataDeletionRequestRepository
{
    private readonly ApplicationDbContext _context;

    public DataDeletionRequestRepository(ApplicationDbContext context) => _context = context;

    public async Task<DataDeletionRequest?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _context.Set<DataDeletionRequest>().FindAsync([id], ct);

    public async Task<IReadOnlyList<DataDeletionRequest>> GetAllAsync(CancellationToken ct = default) =>
        await _context.Set<DataDeletionRequest>().AsNoTracking().ToListAsync(ct);

    public async Task AddAsync(DataDeletionRequest entity, CancellationToken ct = default) =>
        await _context.Set<DataDeletionRequest>().AddAsync(entity, ct);

    public void Update(DataDeletionRequest entity) =>
        _context.Set<DataDeletionRequest>().Update(entity);

    public void Remove(DataDeletionRequest entity) =>
        _context.Set<DataDeletionRequest>().Remove(entity);

    public async Task<DataDeletionRequest?> GetPendingByClientIdAsync(Guid clientId, CancellationToken ct = default) =>
        await _context.Set<DataDeletionRequest>()
            .FirstOrDefaultAsync(d => d.ClientId == clientId && d.Status == DeletionRequestStatus.Pending, ct);
}
