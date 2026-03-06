using AgendeAqui.Application.Abstractions.Messaging;

namespace AgendeAqui.Application.Services.CreateService;

public sealed record CreateServiceCommand(
    string Name,
    int DurationMinutes,
    decimal Price) : ICommand<Guid>;
