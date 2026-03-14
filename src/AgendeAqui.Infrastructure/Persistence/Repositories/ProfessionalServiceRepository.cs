using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Professionals;
using Microsoft.EntityFrameworkCore;

namespace AgendeAqui.Infrastructure.Persistence.Repositories;

internal sealed class ProfessionalServiceRepository(ApplicationDbContext dbContext) : IProfessionalServiceRepository
{
    public async Task<bool> ExistsAsync(Guid professionalId, Guid serviceId, CancellationToken ct = default)
        => await dbContext.Set<ProfessionalService>()
            .AnyAsync(ps => ps.ProfessionalId == professionalId && ps.ServiceId == serviceId, ct);

    public async Task<ProfessionalService?> GetAsync(Guid professionalId, Guid serviceId, CancellationToken ct = default)
        => await dbContext.Set<ProfessionalService>()
            .FirstOrDefaultAsync(ps => ps.ProfessionalId == professionalId && ps.ServiceId == serviceId, ct);

    public async Task<IReadOnlyList<Guid>> GetServiceIdsByProfessionalAsync(Guid professionalId, CancellationToken ct = default)
        => await dbContext.Set<ProfessionalService>()
            .Where(ps => ps.ProfessionalId == professionalId)
            .Select(ps => ps.ServiceId)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Guid>> GetProfessionalIdsByServiceAsync(Guid serviceId, CancellationToken ct = default)
        => await dbContext.Set<ProfessionalService>()
            .Where(ps => ps.ServiceId == serviceId)
            .Select(ps => ps.ProfessionalId)
            .ToListAsync(ct);

    public async Task<bool> HasAnyLinksAsync(Guid professionalId, CancellationToken ct = default)
        => await dbContext.Set<ProfessionalService>()
            .AnyAsync(ps => ps.ProfessionalId == professionalId, ct);

    public async Task AddAsync(ProfessionalService link, CancellationToken ct = default)
        => await dbContext.Set<ProfessionalService>().AddAsync(link, ct);

    public void Remove(ProfessionalService link)
        => dbContext.Set<ProfessionalService>().Remove(link);
}
