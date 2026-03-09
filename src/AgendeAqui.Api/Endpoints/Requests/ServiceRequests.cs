namespace AgendeAqui.Api.Endpoints.Requests;

public sealed record CreateServiceRequest(string Name, int DurationMinutes, decimal Price);
public sealed record UpdateServiceRequest(string Name, int DurationMinutes, decimal Price);
