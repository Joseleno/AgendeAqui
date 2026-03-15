using AgendeAqui.Application.Abstractions.Data;
using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Common;
using Dapper;
using Microsoft.Extensions.Caching.Hybrid;

namespace AgendeAqui.Application.Reports.GetPatientGrowth;

public sealed class GetPatientGrowthQueryHandler(
    ISqlConnectionFactory sqlConnectionFactory,
    ITenantProvider tenantProvider,
    HybridCache cache) : IQueryHandler<GetPatientGrowthQuery, PatientGrowthResponse>
{
    public async ValueTask<Result<PatientGrowthResponse>> Handle(
        GetPatientGrowthQuery query,
        CancellationToken cancellationToken)
    {
        var tenantId = tenantProvider.GetTenantId();
        var cacheKey = $"patient-growth:{tenantId}:{query.Months}";

        var response = await cache.GetOrCreateAsync(
            cacheKey,
            async ct =>
            {
                using var connection = await sqlConnectionFactory.CreateConnectionAsync(ct);

                const string sql = """
                    SELECT TO_CHAR(DATE_TRUNC('month', created_at), 'YYYY-MM') AS Month,
                           CAST(COUNT(*) AS integer) AS NewClients
                    FROM clients
                    WHERE tenant_id = @TenantId
                      AND created_at >= @Since
                    GROUP BY DATE_TRUNC('month', created_at)
                    ORDER BY Month
                    """;

                var since = DateTime.UtcNow.AddMonths(-query.Months);

                var parameters = new DynamicParameters();
                parameters.Add("TenantId", tenantId);
                parameters.Add("Since", since);

                var command = new CommandDefinition(
                    sql,
                    parameters,
                    cancellationToken: ct);

                var rows = await connection.QueryAsync<MonthlyNewClients>(command);
                var monthlyData = rows.AsList();

                var cumulative = 0;
                var points = new List<GrowthPoint>(monthlyData.Count);

                foreach (var row in monthlyData)
                {
                    cumulative += row.NewClients;
                    points.Add(new GrowthPoint(row.Month, row.NewClients, cumulative));
                }

                return new PatientGrowthResponse(points);
            },
            tags: [$"tenant:{tenantId}", "reports"],
            cancellationToken: cancellationToken);

        return Result.Success(response!);
    }

    private sealed record MonthlyNewClients(string Month, int NewClients);
}
