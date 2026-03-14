using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Appointments;
using AgendeAqui.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace AgendeAqui.Infrastructure.Persistence.Repositories;

internal sealed class AppointmentRepository : IAppointmentRepository
{
    private readonly ApplicationDbContext _context;

    public AppointmentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Appointment?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _context.Appointments.FirstOrDefaultAsync(e => e.Id == id, ct);

    public async Task<IReadOnlyList<Appointment>> GetAllAsync(CancellationToken ct = default) =>
        await _context.Appointments.AsNoTracking().ToListAsync(ct);

    public async Task<IReadOnlyList<Appointment>> GetByProfessionalAndDateAsync(
        Guid professionalId,
        DateOnly date,
        CancellationToken ct = default) =>
        await _context.Appointments
            .AsNoTracking()
            .Where(a => a.ProfessionalId == professionalId && a.Date == date)
            .ToListAsync(ct);

    public async Task<bool> HasConflictAsync(
        Guid professionalId,
        DateOnly date,
        TimeOnly start,
        TimeOnly end,
        Guid? excludeAppointmentId = null,
        CancellationToken ct = default)
    {
        var query = _context.Appointments
            .AsNoTracking()
            .Where(a =>
                a.ProfessionalId == professionalId &&
                a.Date == date &&
                a.Status != AppointmentStatus.Cancelled &&
                a.TimeSlot.Start < end &&
                a.TimeSlot.End > start);

        if (excludeAppointmentId.HasValue)
            query = query.Where(a => a.Id != excludeAppointmentId.Value);

        return await query.AnyAsync(ct);
    }

    public async Task<IReadOnlyList<Appointment>> GetByDateRangeAsync(
        Guid professionalId,
        DateOnly from,
        DateOnly to,
        CancellationToken ct = default) =>
        await _context.Appointments
            .AsNoTracking()
            .Where(a =>
                a.ProfessionalId == professionalId &&
                a.Date >= from &&
                a.Date <= to &&
                a.Status != AppointmentStatus.Cancelled)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Appointment>> GetByDateRangeAsync(
        DateOnly from,
        DateOnly to,
        CancellationToken ct = default) =>
        await _context.Appointments
            .AsNoTracking()
            .Where(a => a.Date >= from && a.Date <= to)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Appointment>> GetByDateRangeAndStatusesAsync(
        DateOnly from,
        DateOnly to,
        IEnumerable<AppointmentStatus> statuses,
        CancellationToken ct = default)
    {
        var statusList = statuses.ToList();
        return await _context.Appointments
            .AsNoTracking()
            .Where(a => a.Date >= from && a.Date <= to && statusList.Contains(a.Status))
            .ToListAsync(ct);
    }

    public async Task AddAsync(Appointment entity, CancellationToken ct = default) =>
        await _context.Appointments.AddAsync(entity, ct);

    public void Update(Appointment entity) =>
        _context.Appointments.Update(entity);

    public void Remove(Appointment entity) =>
        _context.Appointments.Remove(entity);
}
