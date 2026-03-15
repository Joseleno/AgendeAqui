using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.InAppNotifications;
using Microsoft.EntityFrameworkCore;

namespace AgendeAqui.Infrastructure.Persistence.Repositories;

internal sealed class InAppNotificationRepository : IInAppNotificationRepository
{
    private readonly ApplicationDbContext _context;

    public InAppNotificationRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<InAppNotification?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _context.InAppNotifications.FirstOrDefaultAsync(e => e.Id == id, ct);

    public async Task<IReadOnlyList<InAppNotification>> GetAllAsync(CancellationToken ct = default) =>
        await _context.InAppNotifications.AsNoTracking().ToListAsync(ct);

    public async Task AddAsync(InAppNotification entity, CancellationToken ct = default) =>
        await _context.InAppNotifications.AddAsync(entity, ct);

    public void Update(InAppNotification entity) =>
        _context.InAppNotifications.Update(entity);

    public void Remove(InAppNotification entity) =>
        _context.InAppNotifications.Remove(entity);

    public async Task<int> GetUnreadCountAsync(Guid userId, CancellationToken ct = default) =>
        await _context.InAppNotifications
            .CountAsync(n => n.UserId == userId && !n.IsRead, ct);

    public async Task MarkAllAsReadAsync(Guid userId, CancellationToken ct = default) =>
        await _context.InAppNotifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ExecuteUpdateAsync(s => s
                .SetProperty(n => n.IsRead, true)
                .SetProperty(n => n.UpdatedAt, DateTime.UtcNow), ct);
}
