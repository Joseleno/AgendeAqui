using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Schedules;
using Microsoft.EntityFrameworkCore;

namespace AgendeAqui.Infrastructure.Persistence.Repositories;

internal sealed class ScheduleRepository : IScheduleRepository
{
    private readonly ApplicationDbContext _context;

    public ScheduleRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Schedule?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _context.Schedules.FindAsync([id], ct);

    public async Task<IReadOnlyList<Schedule>> GetAllAsync(CancellationToken ct = default) =>
        await _context.Schedules.AsNoTracking().ToListAsync(ct);

    public async Task<IReadOnlyList<Schedule>> GetByProfessionalAsync(
        Guid professionalId,
        CancellationToken ct = default) =>
        await _context.Schedules
            .AsNoTracking()
            .Where(s => s.ProfessionalId == professionalId)
            .ToListAsync(ct);

    public async Task<Schedule?> GetByProfessionalAndDayAsync(
        Guid professionalId,
        DayOfWeek dayOfWeek,
        CancellationToken ct = default) =>
        await _context.Schedules
            .AsNoTracking()
            .Where(s => s.ProfessionalId == professionalId && s.DayOfWeek == dayOfWeek && s.IsActive)
            .FirstOrDefaultAsync(ct);

    public async Task AddAsync(Schedule entity, CancellationToken ct = default) =>
        await _context.Schedules.AddAsync(entity, ct);

    public void Update(Schedule entity) =>
        _context.Schedules.Update(entity);

    public void Remove(Schedule entity) =>
        _context.Schedules.Remove(entity);
}
