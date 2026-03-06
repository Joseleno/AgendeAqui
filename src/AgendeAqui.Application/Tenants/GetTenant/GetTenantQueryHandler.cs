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
        using var connection = sqlConnectionFactory.CreateConnection();

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

        var tenant = await connection.QueryFirstOrDefaultAsync<TenantResponse>(
            sql,
            new { query.TenantId });

        if (tenant is null)
            return Result.Failure<TenantResponse>(TenantErrors.NotFound);

        return Result.Success(tenant);
    }
}
