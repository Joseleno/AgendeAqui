using AgendeAqui.Application.Abstractions.Messaging;

namespace AgendeAqui.Application.Professionals.CreateProfessional;

public sealed record CreateProfessionalCommand(
    string Name,
    string Email,
    string Phone,
    string? Specialty = null) : ICommand<Guid>;
