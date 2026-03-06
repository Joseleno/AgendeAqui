namespace AgendeAqui.Application.Professionals.GetProfessional;

public sealed record ProfessionalResponse(
    Guid Id,
    string Name,
    string Email,
    string Phone,
    bool IsActive,
    DateTime CreatedAt);
