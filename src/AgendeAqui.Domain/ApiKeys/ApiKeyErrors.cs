using AgendeAqui.Domain.Common;

namespace AgendeAqui.Domain.ApiKeys;

public static class ApiKeyErrors
{
    public static readonly Error EmptyName = new("ApiKey.EmptyName", "API key name must not be empty.");
    public static readonly Error EmptyKeyHash = new("ApiKey.EmptyKeyHash", "API key hash must not be empty.");
    public static readonly Error NotFound = new("ApiKey.NotFound", "API key not found.");
}
