namespace AgendeAqui.Infrastructure.Notifications;

public sealed class ChakraChatSettings
{
    public string BaseUrl { get; init; } = "https://api.chakrachat.com/v1/";
    public string ApiKey { get; init; } = string.Empty;
}
