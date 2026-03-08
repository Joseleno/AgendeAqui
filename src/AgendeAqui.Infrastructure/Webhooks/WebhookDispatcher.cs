using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Webhooks;
using Microsoft.Extensions.Logging;

namespace AgendeAqui.Infrastructure.Webhooks;

internal sealed class WebhookDispatcher(
    IWebhookRepository webhookRepository,
    IUnitOfWork unitOfWork,
    HttpClient httpClient,
    ILogger<WebhookDispatcher> logger)
{
    public async Task DispatchAsync(string eventType, string payload, CancellationToken ct)
    {
        var webhooks = await webhookRepository.GetActiveByEventAsync(eventType, ct);

        if (webhooks.Count == 0)
        {
            logger.LogDebug("No active webhooks found for event type {EventType}", eventType);
            return;
        }

        var deliveries = new List<WebhookDelivery>();

        foreach (var webhook in webhooks)
        {
            var signature = ComputeSignature(payload, webhook.SecretHash);
            int? responseCode = null;

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, webhook.Url);
                request.Content = new StringContent(payload, Encoding.UTF8);
                request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                request.Headers.Add("X-Webhook-Signature", $"sha256={signature}");

                var response = await httpClient.SendAsync(request, ct);
                responseCode = (int)response.StatusCode;

                logger.LogInformation(
                    "Webhook dispatched to {Url} for event {EventType}, response: {StatusCode}",
                    webhook.Url, eventType, responseCode);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to dispatch webhook to {Url} for event {EventType}", webhook.Url, eventType);
            }

            var deliveryResult = WebhookDelivery.Create(webhook.TenantId, webhook.Id, eventType, payload);
            if (deliveryResult.IsFailure)
            {
                logger.LogWarning("Failed to create WebhookDelivery for {WebhookId}: {Error}",
                    webhook.Id, deliveryResult.Error.Message);
                continue;
            }

            var delivery = deliveryResult.Value;
            if (responseCode.HasValue)
                delivery.RecordResult(responseCode.Value);

            deliveries.Add(delivery);
        }

        foreach (var delivery in deliveries)
            await webhookRepository.AddDeliveryAsync(delivery, ct);

        await unitOfWork.SaveChangesAsync(ct);
    }

    // HMAC key = SHA-256(raw_secret), which is stored as SecretHash.
    // Consumers must compute SHA-256 of their raw secret to derive the same HMAC key.
    // This avoids storing the raw secret while keeping deterministic signatures.
    internal static string ComputeSignature(string payload, string hmacKey)
    {
        var keyBytes = Encoding.UTF8.GetBytes(hmacKey);
        var payloadBytes = Encoding.UTF8.GetBytes(payload);
        var hash = HMACSHA256.HashData(keyBytes, payloadBytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
