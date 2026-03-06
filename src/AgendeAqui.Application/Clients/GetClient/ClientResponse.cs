namespace AgendeAqui.Application.Clients.GetClient;

public sealed record ClientResponse(
    Guid Id,
    string Name,
    string Email,
    string Phone,
    DateTime CreatedAt);
