using AgendeAqui.Application.Abstractions.Data;
using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Application.Tenants.CreateTenant;
using AgendeAqui.Domain.Common;
using Dapper;

namespace AgendeAqui.Application.Tenants.GetTenant;

public sealed class GetTenantQueryHandler(
    ISqlConnectionFactory sqlConnectionFactory) : IQueryHandler<GetTenantQuery, TenantResponse>
{
    public async ValueTask<Result<TenantResponse>> Handle(
        GetTenantQuery query,
        CancellationToken cancellationToken)
    {
        using var connection = await sqlConnectionFactory.CreateConnectionAsync(cancellationToken);

        const string sql = """
            SELECT id AS Id,
                   name AS Name,
                   slug AS Slug,
                   status AS Status,
                   plan AS Plan,
                   created_at AS CreatedAt
            FROM tenants
            WHERE id = @TenantId
            """;

        var command = new CommandDefinition(
            sql,
            new { query.TenantId },
            cancellationToken: cancellationToken);

        var tenant = await connection.QueryFirstOrDefaultAsync<TenantResponse>(command);

        if (tenant is null)
            return Result.Failure<TenantResponse>(TenantErrors.NotFound);

        return Result.Success(tenant);
    }
}
