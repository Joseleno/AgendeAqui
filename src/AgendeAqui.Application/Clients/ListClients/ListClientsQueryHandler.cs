using AgendeAqui.Application.Abstractions.Data;
using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Application.Clients.GetClient;
using AgendeAqui.Application.Common;
using AgendeAqui.Domain.Common;
using Dapper;

namespace AgendeAqui.Application.Clients.ListClients;

public sealed class ListClientsQueryHandler(
    ISqlConnectionFactory sqlConnectionFactory) : IQueryHandler<ListClientsQuery, PagedResponse<ClientResponse>>
{
    public async ValueTask<Result<PagedResponse<ClientResponse>>> Handle(
        ListClientsQuery query,
        CancellationToken cancellationToken)
    {
        using var connection = sqlConnectionFactory.CreateConnection();

        const string countSql = """
            SELECT COUNT(*)
            FROM clients
            """;

        const string itemsSql = """
            SELECT id          AS Id,
                   name        AS Name,
                   email       AS Email,
                   phone       AS Phone,
                   created_at  AS CreatedAt
            FROM clients
            ORDER BY name
            LIMIT @PageSize OFFSET @Offset
            """;

        var totalCount = await connection.ExecuteScalarAsync<int>(countSql);

        var offset = (query.Page - 1) * query.PageSize;

        var items = await connection.QueryAsync<ClientResponse>(
            itemsSql,
            new { query.PageSize, Offset = offset });

        var response = new PagedResponse<ClientResponse>(
            items.AsList(),
            query.Page,
            query.PageSize,
            totalCount);

        return Result.Success(response);
    }
}
