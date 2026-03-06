using AgendeAqui.Application.Abstractions.Data;
using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Clients;
using AgendeAqui.Domain.Common;
using Dapper;

namespace AgendeAqui.Application.Clients.GetClient;

public sealed class GetClientQueryHandler(
    ISqlConnectionFactory sqlConnectionFactory,
    ITenantProvider tenantProvider) : IQueryHandler<GetClientQuery, ClientResponse>
{
    public async ValueTask<Result<ClientResponse>> Handle(
        GetClientQuery query,
        CancellationToken cancellationToken)
    {
        using var connection = sqlConnectionFactory.CreateConnection();

        const string sql = """
            SELECT id          AS Id,
                   name        AS Name,
                   email       AS Email,
                   phone       AS Phone,
                   created_at  AS CreatedAt
            FROM clients
            WHERE id = @ClientId AND tenant_id = @TenantId
            """;

        var command = new CommandDefinition(
            sql,
            new { query.ClientId, TenantId = tenantProvider.GetTenantId() },
            cancellationToken: cancellationToken);

        var client = await connection.QueryFirstOrDefaultAsync<ClientResponse>(command);

        if (client is null)
            return Result.Failure<ClientResponse>(ClientErrors.NotFound);

        return Result.Success(client);
    }
}
