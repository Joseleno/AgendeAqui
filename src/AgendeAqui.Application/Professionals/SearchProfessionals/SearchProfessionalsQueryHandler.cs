using AgendeAqui.Application.Abstractions.Data;
using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Application.Common;
using AgendeAqui.Application.Professionals.GetProfessional;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Common;
using Dapper;
using Microsoft.Extensions.Caching.Hybrid;

namespace AgendeAqui.Application.Professionals.SearchProfessionals;

public sealed class SearchProfessionalsQueryHandler(
    ISqlConnectionFactory sqlConnectionFactory,
    ITenantProvider tenantProvider,
    HybridCache cache) : IQueryHandler<SearchProfessionalsQuery, PagedResponse<ProfessionalResponse>>
{
    public async ValueTask<Result<PagedResponse<ProfessionalResponse>>> Handle(
        SearchProfessionalsQuery query,
        CancellationToken cancellationToken)
    {
        var tenantId = tenantProvider.GetTenantId();

        var nameSuffix = query.Name is not null
            ? $":n{query.Name.ToLowerInvariant().Replace(":", "_")}"
            : "";
        var specialtySuffix = query.Specialty is not null
            ? $":sp{query.Specialty.ToLowerInvariant().Replace(":", "_")}"
            : "";
        var serviceSuffix = query.ServiceId.HasValue
            ? $":svc{query.ServiceId.Value}"
            : "";
        var cacheKey = $"professionals:search:{tenantId}:p{query.Page}:s{query.PageSize}{nameSuffix}{specialtySuffix}{serviceSuffix}";

        var cached = await cache.GetOrCreateAsync(
            cacheKey,
            async ct =>
            {
                using var connection = await sqlConnectionFactory.CreateConnectionAsync(ct);

                var whereClause = "WHERE p.tenant_id = @TenantId AND p.is_active = true";

                if (query.Name is not null)
                    whereClause += " AND LOWER(p.name) LIKE LOWER(@Name)";

                if (query.Specialty is not null)
                    whereClause += " AND LOWER(p.specialty) = LOWER(@Specialty)";

                if (query.ServiceId.HasValue)
                    whereClause += " AND p.id IN (SELECT professional_id FROM professional_services WHERE service_id = @ServiceId AND tenant_id = @TenantId)";

                var countSql = $"""
                    SELECT COUNT(*)
                    FROM professionals p
                    {whereClause}
                    """;

                var itemsSql = $"""
                    SELECT p.id          AS Id,
                           p.name        AS Name,
                           p.email       AS Email,
                           p.phone       AS Phone,
                           p.is_active   AS IsActive,
                           p.specialty   AS Specialty,
                           p.created_at  AS CreatedAt
                    FROM professionals p
                    {whereClause}
                    ORDER BY p.name
                    LIMIT @PageSize OFFSET @Offset
                    """;

                var offset = (query.Page - 1) * query.PageSize;
                var parameters = new
                {
                    TenantId = tenantId,
                    query.PageSize,
                    Offset = offset,
                    Name = query.Name is not null ? $"%{query.Name}%" : null,
                    query.Specialty,
                    ServiceId = query.ServiceId
                };

                var countCommand = new CommandDefinition(countSql, parameters, cancellationToken: ct);
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
