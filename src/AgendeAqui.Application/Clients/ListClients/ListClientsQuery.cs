using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Application.Clients.GetClient;
using AgendeAqui.Application.Common;

namespace AgendeAqui.Application.Clients.ListClients;

public sealed record ListClientsQuery(int Page, int PageSize) : IQuery<PagedResponse<ClientResponse>>;
