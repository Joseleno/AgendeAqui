namespace AgendeAqui.Api.Endpoints.Requests;

public sealed record CreateProfessionalRequest(string Name, string Email, string Phone, string? Specialty = null);
public sealed record UpdateProfessionalRequest(string Name, string Email, string Phone, string? Specialty = null);
public sealed record LinkServiceRequest(Guid ServiceId);
