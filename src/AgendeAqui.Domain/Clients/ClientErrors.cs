using AgendeAqui.Domain.Common;

namespace AgendeAqui.Domain.Clients;

public static class ClientErrors
{
    public static readonly Error NotFound = new("Client.NotFound", "Client not found.");
    public static readonly Error InvalidName = new("Client.InvalidName", "Client name is required.");
    public static readonly Error EmailAlreadyExists = new("Client.EmailAlreadyExists", "A client with this email already exists.");
    public static readonly Error PhoneAlreadyExists = new("Client.PhoneAlreadyExists", "A client with this phone number already exists.");
}
