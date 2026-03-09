using AgendeAqui.Application.Abstractions.Messaging;

namespace AgendeAqui.Application.Clients.DeleteClientData;

public sealed record DeleteClientDataCommand(Guid ClientId) : ICommand;
