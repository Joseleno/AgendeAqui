using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.ClinicalNotes;
using Microsoft.EntityFrameworkCore;

namespace AgendeAqui.Infrastructure.Persistence.Repositories;

internal sealed class ClinicalNoteRepository : IClinicalNoteRepository
{
    private readonly ApplicationDbContext _context;

    public ClinicalNoteRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ClinicalNote?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _context.ClinicalNotes.FirstOrDefaultAsync(e => e.Id == id, ct);

    public async Task<IReadOnlyList<ClinicalNote>> GetAllAsync(CancellationToken ct = default) =>
        await _context.ClinicalNotes.AsNoTracking().ToListAsync(ct);

    public async Task<IReadOnlyList<ClinicalNote>> GetByClientIdAsync(
        Guid clientId,
        int page,
        int pageSize,
        CancellationToken ct = default) =>
        await _context.ClinicalNotes
            .AsNoTracking()
            .Where(cn => cn.ClientId == clientId)
            .OrderByDescending(cn => cn.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

    public async Task AddAsync(ClinicalNote entity, CancellationToken ct = default) =>
        await _context.ClinicalNotes.AddAsync(entity, ct);

    public void Update(ClinicalNote entity) =>
        _context.ClinicalNotes.Update(entity);

    public void Remove(ClinicalNote entity) =>
        _context.ClinicalNotes.Remove(entity);
}
