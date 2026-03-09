namespace AgendeAqui.Application.Abstractions.Notifications;

public interface IWhatsAppClient
{
    Task<bool> SendTemplateMessageAsync(string phoneNumber, string templateName, Dictionary<string, string> parameters, CancellationToken ct = default);
}
