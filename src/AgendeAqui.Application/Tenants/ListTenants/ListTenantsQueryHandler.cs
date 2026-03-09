using AgendeAqui.Application.Abstractions.Data;
using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Application.Common;
using AgendeAqui.Application.Tenants.GetTenant;
using AgendeAqui.Domain.Common;
using AgendeAqui.Domain.Tenants;
using Dapper;

namespace AgendeAqui.Application.Tenants.ListTenants;

public sealed class ListTenantsQueryHandler(
    ISqlConnectionFactory sqlConnectionFactory) : IQueryHandler<ListTenantsQuery, PagedResponse<TenantResponse>>
{
    public async ValueTask<Result<PagedResponse<TenantResponse>>> Handle(
        ListTenantsQuery query,
        CancellationToken cancellationToken)
    {
        using var connection = await sqlConnectionFactory.CreateConnectionAsync(cancellationToken);

        var whereClause = string.Empty;
        int? statusFilter = null;

        if (!string.IsNullOrWhiteSpace(query.Status) &&
            Enum.TryParse<TenantStatus>(query.Status, true, out var status))
        {
            whereClause = "WHERE status = @Status";
            statusFilter = (int)status;
        }

        var countSql = $"SELECT COUNT(*) FROM tenants {whereClause}";
        var itemsSql = $"""
            SELECT id         AS Id,
                   name       AS Name,
                   slug       AS Slug,
                   status     AS Status,
                   plan       AS Plan,
                   created_at AS CreatedAt
            FROM tenants
            {whereClause}
            ORDER BY created_at DESC
            LIMIT @PageSize OFFSET @Offset
            """;

        var offset = (query.Page - 1) * query.PageSize;
        var parameters = new { Status = statusFilter, query.PageSize, Offset = offset };

        var countCommand = new CommandDefinition(countSql, parameters, cancellationToken: cancellationToken);
        var totalCount = await connection.ExecuteScalarAsync<int>(countCommand);

        var itemsCommand = new CommandDefinition(itemsSql, parameters, cancellationToken: cancellationToken);
        var rows = await connection.QueryAsync<TenantRow>(itemsCommand);

        var items = rows.Select(r => new TenantResponse(
            r.Id,
            r.Name,
            r.Slug,
            ((TenantStatus)r.Status).ToString(),
            ((TenantPlan)r.Plan).ToString(),
            r.CreatedAt)).AsList();

        return Result.Success(new PagedResponse<TenantResponse>(
            items, query.Page, query.PageSize, totalCount));
    }

    private sealed record TenantRow(
        Guid Id,
        string Name,
        string Slug,
        int Status,
        int Plan,
        DateTime CreatedAt);
}
