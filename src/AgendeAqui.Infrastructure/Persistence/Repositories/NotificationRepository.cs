using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Notifications;
using Microsoft.EntityFrameworkCore;

namespace AgendeAqui.Infrastructure.Persistence.Repositories;

internal sealed class NotificationRepository : INotificationRepository
{
    private readonly ApplicationDbContext _context;

    public NotificationRepository(ApplicationDbContext context) => _context = context;

    public async Task<Notification?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _context.Set<Notification>().FindAsync([id], ct);

    public async Task<IReadOnlyList<Notification>> GetAllAsync(CancellationToken ct = default) =>
        await _context.Set<Notification>().AsNoTracking().ToListAsync(ct);

    public async Task AddAsync(Notification notification, CancellationToken ct = default) =>
        await _context.Set<Notification>().AddAsync(notification, ct);

    public void Update(Notification notification) =>
        _context.Set<Notification>().Update(notification);

    public void Remove(Notification notification) =>
        _context.Set<Notification>().Remove(notification);

    public async Task<List<Notification>> GetByAppointmentIdAsync(Guid appointmentId, CancellationToken ct = default) =>
        await _context.Set<Notification>()
            .AsNoTracking()
            .Where(n => n.AppointmentId == appointmentId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(ct);
}
