using AgendeAqui.Application.Abstractions.Data;
using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Application.Clients.GetClient;
using AgendeAqui.Application.Common;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Common;
using Dapper;

namespace AgendeAqui.Application.Clients.ListClients;

public sealed class ListClientsQueryHandler(
    ISqlConnectionFactory sqlConnectionFactory,
    ITenantProvider tenantProvider) : IQueryHandler<ListClientsQuery, PagedResponse<ClientResponse>>
{
    public async ValueTask<Result<PagedResponse<ClientResponse>>> Handle(
        ListClientsQuery query,
        CancellationToken cancellationToken)
    {
        using var connection = await sqlConnectionFactory.CreateConnectionAsync(cancellationToken);

        var tenantId = tenantProvider.GetTenantId();

        const string countSql = """
            SELECT COUNT(*)
            FROM clients
            WHERE tenant_id = @TenantId
            """;

        const string itemsSql = """
            SELECT id          AS Id,
                   name        AS Name,
                   email       AS Email,
                   phone       AS Phone,
                   created_at  AS CreatedAt
            FROM clients
            WHERE tenant_id = @TenantId
            ORDER BY name
            LIMIT @PageSize OFFSET @Offset
            """;

        var offset = (query.Page - 1) * query.PageSize;
        var parameters = new { TenantId = tenantId, query.PageSize, Offset = offset };

        var countCommand = new CommandDefinition(countSql, new { TenantId = tenantId }, cancellationToken: cancellationToken);
        var totalCount = await connection.ExecuteScalarAsync<int>(countCommand);

        var itemsCommand = new CommandDefinition(itemsSql, parameters, cancellationToken: cancellationToken);
        var items = await connection.QueryAsync<ClientResponse>(itemsCommand);

        var response = new PagedResponse<ClientResponse>(
            items.AsList(),
            query.Page,
            query.PageSize,
            totalCount);

        return Result.Success(response);
    }
}
