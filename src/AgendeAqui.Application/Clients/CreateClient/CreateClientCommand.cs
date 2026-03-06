using AgendeAqui.Application.Abstractions.Messaging;

namespace AgendeAqui.Application.Clients.CreateClient;

public sealed record CreateClientCommand(
    string Name,
    string Email,
    string Phone) : ICommand<Guid>;
