using AgendeAqui.Application.Abstractions.Data;
using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Application.Common;
using AgendeAqui.Application.Professionals.GetProfessional;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Common;
using Dapper;
using Microsoft.Extensions.Caching.Hybrid;

namespace AgendeAqui.Application.Professionals.ListProfessionals;

public sealed class ListProfessionalsQueryHandler(
    ISqlConnectionFactory sqlConnectionFactory,
    ITenantProvider tenantProvider,
    HybridCache cache) : IQueryHandler<ListProfessionalsQuery, PagedResponse<ProfessionalResponse>>
{
    public async ValueTask<Result<PagedResponse<ProfessionalResponse>>> Handle(
        ListProfessionalsQuery query,
        CancellationToken cancellationToken)
    {
        var tenantId = tenantProvider.GetTenantId();
        var cacheKey = $"professionals:{tenantId}:p{query.Page}:s{query.PageSize}";

        var cached = await cache.GetOrCreateAsync(
            cacheKey,
            async ct =>
            {
                using var connection = await sqlConnectionFactory.CreateConnectionAsync(ct);

                const string countSql = """
                    SELECT COUNT(*)
                    FROM professionals
                    WHERE tenant_id = @TenantId AND is_active = true
                    """;

                const string itemsSql = """
                    SELECT id          AS Id,
                           name        AS Name,
                           email       AS Email,
                           phone       AS Phone,
                           is_active   AS IsActive,
                           created_at  AS CreatedAt
                    FROM professionals
                    WHERE tenant_id = @TenantId AND is_active = true
                    ORDER BY name
                    LIMIT @PageSize OFFSET @Offset
                    """;

                var offset = (query.Page - 1) * query.PageSize;
                var parameters = new { TenantId = tenantId, query.PageSize, Offset = offset };

                var countCommand = new CommandDefinition(countSql, new { TenantId = tenantId }, cancellationToken: ct);
                var totalCount = await connection.ExecuteScalarAsync<int>(countCommand);

                var itemsCommand = new CommandDefinition(itemsSql, parameters, cancellationToken: ct);
                var items = await connection.QueryAsync<ProfessionalResponse>(itemsCommand);

                return new PagedResponse<ProfessionalResponse>(
                    items.AsList(),
                    query.Page,
                    query.PageSize,
                    totalCount);
            },
            tags: [$"tenant:{tenantId}", "professionals"],
            cancellationToken: cancellationToken);

        return Result.Success(cached!);
    }
}
