using AgendeAqui.Application.Abstractions.Messaging;

namespace AgendeAqui.Application.Professionals.UnlinkService;

public sealed record UnlinkServiceFromProfessionalCommand(
    Guid ProfessionalId,
    Guid ServiceId) : ICommand;
