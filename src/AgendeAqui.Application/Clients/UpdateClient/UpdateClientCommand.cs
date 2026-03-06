using AgendeAqui.Application.Abstractions.Messaging;

namespace AgendeAqui.Application.Clients.UpdateClient;

public sealed record UpdateClientCommand(
    Guid ClientId,
    string Name,
    string Email,
    string Phone) : ICommand;
