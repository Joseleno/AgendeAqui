using AgendeAqui.Application.Abstractions.Data;
using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Domain.Common;
using AgendeAqui.Domain.Tenants;
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

        var row = await connection.QueryFirstOrDefaultAsync<TenantRow>(command);

        if (row is null)
            return Result.Failure<TenantResponse>(TenantErrors.NotFound);

        var response = new TenantResponse(
            row.Id,
            row.Name,
            row.Slug,
            ((TenantStatus)row.Status).ToString(),
            ((TenantPlan)row.Plan).ToString(),
            row.CreatedAt);

        return Result.Success(response);
    }

    private sealed record TenantRow(
        Guid Id,
        string Name,
        string Slug,
        int Status,
        int Plan,
        DateTime CreatedAt);
}
