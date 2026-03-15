using AgendeAqui.Application.Abstractions.Data;
using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Common;
using Dapper;
using Microsoft.Extensions.Caching.Hybrid;

namespace AgendeAqui.Application.Reports.GetRevenueTimeline;

public sealed class GetRevenueTimelineQueryHandler(
    ISqlConnectionFactory sqlConnectionFactory,
    ITenantProvider tenantProvider,
    HybridCache cache) : IQueryHandler<GetRevenueTimelineQuery, RevenueTimelineResponse>
{
    public async ValueTask<Result<RevenueTimelineResponse>> Handle(
        GetRevenueTimelineQuery query,
        CancellationToken cancellationToken)
    {
        var tenantId = tenantProvider.GetTenantId();

        var response = await cache.GetOrCreateAsync(
            $"revenue-timeline:{tenantId}:{query.From}:{query.To}",
            async ct =>
            {
                using var connection = await sqlConnectionFactory.CreateConnectionAsync(ct);

                const string sql = """
                    SELECT TO_CHAR(DATE_TRUNC('month', a.date), 'YYYY-MM') AS Month,
                           CAST(SUM(s.price) AS numeric) AS Revenue,
                           CAST(COUNT(*) AS integer) AS AppointmentCount
                    FROM appointments a
                    INNER JOIN services s ON s.id = a.service_id AND s.tenant_id = @TenantId
                    WHERE a.tenant_id = @TenantId
                      AND a.date BETWEEN @From AND @To
                      AND a.status = 'Completed'
                    GROUP BY DATE_TRUNC('month', a.date)
                    ORDER BY Month
                    """;

                var parameters = new DynamicParameters();
                parameters.Add("TenantId", tenantId);
                parameters.Add("From", query.From.ToDateTime(TimeOnly.MinValue), System.Data.DbType.Date);
                parameters.Add("To", query.To.ToDateTime(TimeOnly.MinValue), System.Data.DbType.Date);

                var command = new CommandDefinition(
                    sql,
                    parameters,
                    cancellationToken: ct);

                var rows = await connection.QueryAsync<RevenuePoint>(command);

                return new RevenueTimelineResponse(rows.AsList());
            },
            tags: [$"tenant:{tenantId}", "reports"],
            cancellationToken: cancellationToken);

        return Result.Success(response!);
    }
}
