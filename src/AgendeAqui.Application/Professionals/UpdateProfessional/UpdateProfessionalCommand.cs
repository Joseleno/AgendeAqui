using AgendeAqui.Application.Abstractions.Messaging;

namespace AgendeAqui.Application.Professionals.UpdateProfessional;

public sealed record UpdateProfessionalCommand(
    Guid ProfessionalId,
    string Name,
    string Email,
    string Phone,
    string? Specialty = null) : ICommand;
