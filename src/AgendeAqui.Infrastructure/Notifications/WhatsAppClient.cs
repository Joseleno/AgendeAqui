using System.Net.Http.Json;
using AgendeAqui.Application.Abstractions.Notifications;
using Microsoft.Extensions.Logging;

namespace AgendeAqui.Infrastructure.Notifications;

internal sealed class WhatsAppClient : IWhatsAppClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<WhatsAppClient> _logger;

    public WhatsAppClient(HttpClient httpClient, ILogger<WhatsAppClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<bool> SendTemplateMessageAsync(
        string phoneNumber,
        string templateName,
        Dictionary<string, string> parameters,
        CancellationToken ct = default)
    {
        var payload = new
        {
            phone = phoneNumber,
            template = templateName,
            parameters
        };

        _logger.LogInformation("Sending WhatsApp template {Template} to {Phone}", templateName, phoneNumber);

        var response = await _httpClient.PostAsJsonAsync("messages/template", payload, ct);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("WhatsApp API returned {StatusCode} for {Phone}", response.StatusCode, phoneNumber);
            return false;
        }

        return true;
    }
}
