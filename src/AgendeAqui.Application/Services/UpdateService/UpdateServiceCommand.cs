using AgendeAqui.Application.Abstractions.Messaging;

namespace AgendeAqui.Application.Services.UpdateService;

public sealed record UpdateServiceCommand(
    Guid ServiceId,
    string Name,
    int DurationMinutes,
    decimal Price) : ICommand;
