using AgendeAqui.Domain.Common;

namespace AgendeAqui.Domain.Webhooks;

public static class WebhookErrors
{
    public static readonly Error InvalidTenant = new("Webhook.InvalidTenant", "Tenant ID is required.");
    public static readonly Error InvalidWebhookId = new("Webhook.InvalidWebhookId", "Webhook ID is required.");
    public static readonly Error InvalidUrl = new("Webhook.InvalidUrl", "A valid HTTP or HTTPS URL is required (max 2048 characters).");
    public static readonly Error InvalidSecret = new("Webhook.InvalidSecret", "A non-empty secret is required.");
    public static readonly Error InvalidEvents = new("Webhook.InvalidEvents", "At least one event type must be specified.");
    public static readonly Error InvalidPayload = new("Webhook.InvalidPayload", "Payload is required.");
    public static readonly Error PayloadTooLarge = new("Webhook.PayloadTooLarge", "Payload exceeds the maximum allowed size of 64 KB.");
    public static readonly Error NotFound = new("Webhook.NotFound", "Webhook not found.");
}
