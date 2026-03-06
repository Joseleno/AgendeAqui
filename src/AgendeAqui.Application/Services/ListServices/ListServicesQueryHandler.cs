using AgendeAqui.Application.Abstractions.Data;
using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Application.Common;
using AgendeAqui.Application.Services.GetService;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Common;
using Dapper;

namespace AgendeAqui.Application.Services.ListServices;

public sealed class ListServicesQueryHandler(
    ISqlConnectionFactory sqlConnectionFactory,
    ITenantProvider tenantProvider) : IQueryHandler<ListServicesQuery, PagedResponse<ServiceResponse>>
{
    public async ValueTask<Result<PagedResponse<ServiceResponse>>> Handle(
        ListServicesQuery query,
        CancellationToken cancellationToken)
    {
        using var connection = await sqlConnectionFactory.CreateConnectionAsync(cancellationToken);

        var tenantId = tenantProvider.GetTenantId();

        const string countSql = """
            SELECT COUNT(*)
            FROM services
            WHERE tenant_id = @TenantId AND is_active = true
            """;

        const string itemsSql = """
            SELECT id AS Id,
                   name AS Name,
                   EXTRACT(EPOCH FROM duration) / 60 AS DurationMinutes,
                   price AS Price,
                   is_active AS IsActive,
                   created_at AS CreatedAt
            FROM services
            WHERE tenant_id = @TenantId AND is_active = true
            ORDER BY name
            LIMIT @PageSize OFFSET @Offset
            """;

        var offset = (query.Page - 1) * query.PageSize;
        var parameters = new { TenantId = tenantId, query.PageSize, Offset = offset };

        var countCommand = new CommandDefinition(countSql, new { TenantId = tenantId }, cancellationToken: cancellationToken);
        var totalCount = await connection.ExecuteScalarAsync<int>(countCommand);

        var itemsCommand = new CommandDefinition(itemsSql, parameters, cancellationToken: cancellationToken);
        var items = await connection.QueryAsync<ServiceResponse>(itemsCommand);

        var response = new PagedResponse<ServiceResponse>(
            items.AsList(),
            query.Page,
            query.PageSize,
            totalCount);

        return Result.Success(response);
    }
}
