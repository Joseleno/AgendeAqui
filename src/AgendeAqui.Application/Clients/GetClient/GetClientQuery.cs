using AgendeAqui.Application.Abstractions.Messaging;

namespace AgendeAqui.Application.Clients.GetClient;

public sealed record GetClientQuery(Guid ClientId) : IQuery<ClientResponse>;
