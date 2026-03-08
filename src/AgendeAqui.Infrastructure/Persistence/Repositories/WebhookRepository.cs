using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Webhooks;
using Microsoft.EntityFrameworkCore;

namespace AgendeAqui.Infrastructure.Persistence.Repositories;

internal sealed class WebhookRepository : IWebhookRepository
{
    private readonly ApplicationDbContext _context;

    public WebhookRepository(ApplicationDbContext context) => _context = context;

    public async Task<Webhook?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _context.Set<Webhook>().FindAsync([id], ct);

    public async Task<IReadOnlyList<Webhook>> GetAllAsync(CancellationToken ct = default) =>
        await _context.Set<Webhook>().AsNoTracking().ToListAsync(ct);

    public async Task AddAsync(Webhook entity, CancellationToken ct = default) =>
        await _context.Set<Webhook>().AddAsync(entity, ct);

    public void Update(Webhook entity) =>
        _context.Set<Webhook>().Update(entity);

    public void Remove(Webhook entity) =>
        _context.Set<Webhook>().Remove(entity);

    public async Task<List<Webhook>> GetActiveByEventAsync(string eventType, CancellationToken ct = default) =>
        await _context.Set<Webhook>()
            .AsNoTracking()
            .Where(w => w.IsActive && w.Events.Contains(eventType))
            .ToListAsync(ct);

    public async Task<List<WebhookDelivery>> GetDeliveriesByWebhookIdAsync(Guid webhookId, CancellationToken ct = default) =>
        await _context.Set<WebhookDelivery>()
            .AsNoTracking()
            .Where(d => d.WebhookId == webhookId)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync(ct);

    public async Task AddDeliveryAsync(WebhookDelivery delivery, CancellationToken ct = default) =>
        await _context.Set<WebhookDelivery>().AddAsync(delivery, ct);
}
