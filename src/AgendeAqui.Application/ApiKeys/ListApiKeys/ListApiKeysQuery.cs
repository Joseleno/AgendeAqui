using AgendeAqui.Application.Abstractions.Messaging;

namespace AgendeAqui.Application.ApiKeys.ListApiKeys;

public sealed record ListApiKeysQuery : IQuery<List<ApiKeyResponse>>;
