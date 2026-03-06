using AgendeAqui.Application.Abstractions.Data;
using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Domain.Clients;
using AgendeAqui.Domain.Common;
using Dapper;

namespace AgendeAqui.Application.Clients.GetClient;

public sealed class GetClientQueryHandler(
    ISqlConnectionFactory sqlConnectionFactory) : IQueryHandler<GetClientQuery, ClientResponse>
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
            WHERE id = @ClientId
            """;

        var client = await connection.QueryFirstOrDefaultAsync<ClientResponse>(
            sql,
            new { query.ClientId });

        if (client is null)
            return Result.Failure<ClientResponse>(ClientErrors.NotFound);

        return Result.Success(client);
    }
}
