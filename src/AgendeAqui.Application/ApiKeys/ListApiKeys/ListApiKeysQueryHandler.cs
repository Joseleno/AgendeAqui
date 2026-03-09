using AgendeAqui.Application.Abstractions.Data;
using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Common;
using Dapper;

namespace AgendeAqui.Application.ApiKeys.ListApiKeys;

internal sealed class ListApiKeysQueryHandler(
    ISqlConnectionFactory sqlConnectionFactory,
    ITenantProvider tenantProvider) : IQueryHandler<ListApiKeysQuery, List<ApiKeyResponse>>
{
    public async ValueTask<Result<List<ApiKeyResponse>>> Handle(
        ListApiKeysQuery query,
        CancellationToken cancellationToken)
    {
        using var connection = await sqlConnectionFactory.CreateConnectionAsync(cancellationToken);

        var tenantId = tenantProvider.GetTenantId();

        const string sql = """
            SELECT id          AS Id,
                   name        AS Name,
                   LEFT(key_hash, 8) AS KeyPrefix,
                   is_active   AS IsActive,
                   expires_at  AS ExpiresAt,
                   created_at  AS CreatedAt
            FROM api_keys
            WHERE tenant_id = @TenantId
            ORDER BY created_at DESC
            """;

        var command = new CommandDefinition(sql, new { TenantId = tenantId }, cancellationToken: cancellationToken);
        var rows = await connection.QueryAsync<ApiKeyResponse>(command);

        return Result.Success(rows.ToList());
    }
}
