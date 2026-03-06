using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Appointments;
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
        await _context.Appointments.FindAsync([id], ct);

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
        CancellationToken ct = default) =>
        await _context.Appointments
            .AsNoTracking()
            .AnyAsync(a =>
                a.ProfessionalId == professionalId &&
                a.Date == date &&
                a.TimeSlot.Start < end &&
                a.TimeSlot.End > start,
                ct);

    public async Task AddAsync(Appointment entity, CancellationToken ct = default) =>
        await _context.Appointments.AddAsync(entity, ct);

    public void Update(Appointment entity) =>
        _context.Appointments.Update(entity);

    public void Remove(Appointment entity) =>
        _context.Appointments.Remove(entity);
}
