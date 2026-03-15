using AgendeAqui.Application.Abstractions.Data;
using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Common;
using Dapper;
using Microsoft.Extensions.Caching.Hybrid;

namespace AgendeAqui.Application.Reports.GetBusiestHours;

public sealed class GetBusiestHoursQueryHandler(
    ISqlConnectionFactory sqlConnectionFactory,
    ITenantProvider tenantProvider,
    HybridCache cache) : IQueryHandler<GetBusiestHoursQuery, BusiestHoursResponse>
{
    public async ValueTask<Result<BusiestHoursResponse>> Handle(
        GetBusiestHoursQuery query,
        CancellationToken cancellationToken)
    {
        var tenantId = tenantProvider.GetTenantId();

        var response = await cache.GetOrCreateAsync(
            $"busiest-hours:{tenantId}:{query.From}:{query.To}",
            async ct =>
            {
                using var connection = await sqlConnectionFactory.CreateConnectionAsync(ct);

                const string sql = """
                    SELECT EXTRACT(DOW FROM date)::integer AS DayOfWeek,
                           EXTRACT(HOUR FROM start_time)::integer AS Hour,
                           CAST(COUNT(*) AS integer) AS Count
                    FROM appointments
                    WHERE tenant_id = @TenantId AND date BETWEEN @From AND @To
                      AND status NOT IN ('Cancelled')
                    GROUP BY EXTRACT(DOW FROM date), EXTRACT(HOUR FROM start_time)
                    ORDER BY DayOfWeek, Hour
                    """;

                var parameters = new DynamicParameters();
                parameters.Add("TenantId", tenantId);
                parameters.Add("From", query.From.ToDateTime(TimeOnly.MinValue), System.Data.DbType.Date);
                parameters.Add("To", query.To.ToDateTime(TimeOnly.MinValue), System.Data.DbType.Date);

                var command = new CommandDefinition(
                    sql,
                    parameters,
                    cancellationToken: ct);

                var rows = await connection.QueryAsync<HourSlot>(command);

                return new BusiestHoursResponse(rows.AsList());
            },
            tags: [$"tenant:{tenantId}", "reports"],
            cancellationToken: cancellationToken);

        return Result.Success(response!);
    }
}
