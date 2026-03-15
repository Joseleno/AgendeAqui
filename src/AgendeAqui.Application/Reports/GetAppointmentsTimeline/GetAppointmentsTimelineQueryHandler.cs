using AgendeAqui.Application.Abstractions.Data;
using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Common;
using Dapper;
using Microsoft.Extensions.Caching.Hybrid;

namespace AgendeAqui.Application.Reports.GetAppointmentsTimeline;

public sealed class GetAppointmentsTimelineQueryHandler(
    ISqlConnectionFactory sqlConnectionFactory,
    ITenantProvider tenantProvider,
    HybridCache cache) : IQueryHandler<GetAppointmentsTimelineQuery, AppointmentsTimelineResponse>
{
    public async ValueTask<Result<AppointmentsTimelineResponse>> Handle(
        GetAppointmentsTimelineQuery query,
        CancellationToken cancellationToken)
    {
        var tenantId = tenantProvider.GetTenantId();
        var cacheKey = $"appointments-timeline:{tenantId}:{query.From}:{query.To}:{query.GroupBy}";

        var response = await cache.GetOrCreateAsync(
            cacheKey,
            async ct =>
            {
                using var connection = await sqlConnectionFactory.CreateConnectionAsync(ct);

                const string daySql = """
                    SELECT TO_CHAR(date, 'YYYY-MM-DD') AS Period,
                           CAST(COUNT(*) FILTER (WHERE status = 'Scheduled') AS integer) AS Scheduled,
                           CAST(COUNT(*) FILTER (WHERE status = 'Completed') AS integer) AS Completed,
                           CAST(COUNT(*) FILTER (WHERE status = 'Cancelled') AS integer) AS Cancelled,
                           CAST(COUNT(*) FILTER (WHERE status = 'NoShow') AS integer) AS NoShow
                    FROM appointments
                    WHERE tenant_id = @TenantId AND date BETWEEN @From AND @To
                    GROUP BY date
                    ORDER BY Period
                    """;

                const string weekSql = """
                    SELECT TO_CHAR(DATE_TRUNC('week', date), 'YYYY-MM-DD') AS Period,
                           CAST(COUNT(*) FILTER (WHERE status = 'Scheduled') AS integer) AS Scheduled,
                           CAST(COUNT(*) FILTER (WHERE status = 'Completed') AS integer) AS Completed,
                           CAST(COUNT(*) FILTER (WHERE status = 'Cancelled') AS integer) AS Cancelled,
                           CAST(COUNT(*) FILTER (WHERE status = 'NoShow') AS integer) AS NoShow
                    FROM appointments
                    WHERE tenant_id = @TenantId AND date BETWEEN @From AND @To
                    GROUP BY DATE_TRUNC('week', date)
                    ORDER BY Period
                    """;

                const string monthSql = """
                    SELECT TO_CHAR(DATE_TRUNC('month', date), 'YYYY-MM') AS Period,
                           CAST(COUNT(*) FILTER (WHERE status = 'Scheduled') AS integer) AS Scheduled,
                           CAST(COUNT(*) FILTER (WHERE status = 'Completed') AS integer) AS Completed,
                           CAST(COUNT(*) FILTER (WHERE status = 'Cancelled') AS integer) AS Cancelled,
                           CAST(COUNT(*) FILTER (WHERE status = 'NoShow') AS integer) AS NoShow
                    FROM appointments
                    WHERE tenant_id = @TenantId AND date BETWEEN @From AND @To
                    GROUP BY DATE_TRUNC('month', date)
                    ORDER BY Period
                    """;

                var sql = query.GroupBy switch
                {
                    "week" => weekSql,
                    "month" => monthSql,
                    _ => daySql
                };

                var parameters = new DynamicParameters();
                parameters.Add("TenantId", tenantId);
                parameters.Add("From", query.From.ToDateTime(TimeOnly.MinValue), System.Data.DbType.Date);
                parameters.Add("To", query.To.ToDateTime(TimeOnly.MinValue), System.Data.DbType.Date);

                var command = new CommandDefinition(
                    sql,
                    parameters,
                    cancellationToken: ct);

                var rows = await connection.QueryAsync<TimelinePoint>(command);

                return new AppointmentsTimelineResponse(rows.AsList());
            },
            tags: [$"tenant:{tenantId}", "reports"],
            cancellationToken: cancellationToken);

        return Result.Success(response!);
    }
}
