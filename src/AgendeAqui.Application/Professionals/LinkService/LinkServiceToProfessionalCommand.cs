using AgendeAqui.Application.Abstractions.Messaging;

namespace AgendeAqui.Application.Professionals.LinkService;

public sealed record LinkServiceToProfessionalCommand(
    Guid ProfessionalId,
    Guid ServiceId) : ICommand;
