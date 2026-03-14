using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Schedules;
using Microsoft.EntityFrameworkCore;

namespace AgendeAqui.Infrastructure.Persistence.Repositories;

internal sealed class AbsenceRepository(ApplicationDbContext dbContext) : IAbsenceRepository
{
    public async Task<Absence?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await dbContext.Set<Absence>().FirstOrDefaultAsync(a => a.Id == id, ct);

    public async Task<IReadOnlyList<Absence>> GetAllAsync(CancellationToken ct = default)
        => await dbContext.Set<Absence>().ToListAsync(ct);

    public async Task<IReadOnlyList<Absence>> GetByProfessionalAndDateRangeAsync(
        Guid professionalId, DateOnly from, DateOnly to, CancellationToken ct = default)
        => await dbContext.Set<Absence>()
            .Where(a => a.ProfessionalId == professionalId && a.Date >= from && a.Date <= to)
            .OrderBy(a => a.Date)
            .ThenBy(a => a.StartTime)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Absence>> GetByProfessionalAndDateAsync(
        Guid professionalId, DateOnly date, CancellationToken ct = default)
        => await dbContext.Set<Absence>()
            .Where(a => a.ProfessionalId == professionalId && a.Date == date)
            .ToListAsync(ct);

    public async Task AddAsync(Absence entity, CancellationToken ct = default)
        => await dbContext.Set<Absence>().AddAsync(entity, ct);

    public void Update(Absence entity)
        => dbContext.Set<Absence>().Update(entity);

    public void Remove(Absence entity)
        => dbContext.Set<Absence>().Remove(entity);
}
