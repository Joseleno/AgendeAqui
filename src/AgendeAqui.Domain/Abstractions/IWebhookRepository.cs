using AgendeAqui.Domain.Webhooks;

namespace AgendeAqui.Domain.Abstractions;

public interface IWebhookRepository : IRepository<Webhook>
{
    Task<List<Webhook>> GetActiveByEventAsync(string eventType, CancellationToken ct = default);
    Task<List<WebhookDelivery>> GetDeliveriesByWebhookIdAsync(Guid webhookId, CancellationToken ct = default);
    Task AddDeliveryAsync(WebhookDelivery delivery, CancellationToken ct = default);
}
