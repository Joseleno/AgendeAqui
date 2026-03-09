using AgendeAqui.Application.Abstractions.Messaging;

namespace AgendeAqui.Application.Clients.ExportClientData;

public sealed record ExportClientDataQuery(Guid ClientId) : IQuery<ClientDataExport>;
