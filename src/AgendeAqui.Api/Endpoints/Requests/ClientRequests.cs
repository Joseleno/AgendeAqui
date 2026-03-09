namespace AgendeAqui.Api.Endpoints.Requests;

public sealed record CreateClientRequest(string Name, string Email, string Phone);
public sealed record UpdateClientRequest(string Name, string Email, string Phone);
