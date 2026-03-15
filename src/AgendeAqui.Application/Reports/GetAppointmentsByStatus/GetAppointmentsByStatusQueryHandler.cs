using AgendeAqui.Application.Abstractions.Data;
using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Common;
using Dapper;
using Microsoft.Extensions.Caching.Hybrid;

namespace AgendeAqui.Application.Reports.GetAppointmentsByStatus;

public sealed class GetAppointmentsByStatusQueryHandler(
    ISqlConnectionFactory sqlConnectionFactory,
    ITenantProvider tenantProvider,
    HybridCache cache) : IQueryHandler<GetAppointmentsByStatusQuery, AppointmentsByStatusResponse>
{
    public async ValueTask<Result<AppointmentsByStatusResponse>> Handle(
        GetAppointmentsByStatusQuery query,
        CancellationToken cancellationToken)
    {
        var tenantId = tenantProvider.GetTenantId();
        var cacheKey = $"appointments-by-status:{tenantId}:{query.From}:{query.To}";

        var response = await cache.GetOrCreateAsync(
            cacheKey,
            async ct =>
            {
                using var connection = await sqlConnectionFactory.CreateConnectionAsync(ct);

                const string sql = """
                    SELECT status AS Status,
                           CAST(COUNT(*) AS integer) AS Count
                    FROM appointments
                    WHERE tenant_id = @TenantId AND date BETWEEN @From AND @To
                    GROUP BY status
                    """;

                var parameters = new DynamicParameters();
                parameters.Add("TenantId", tenantId);
                parameters.Add("From", query.From.ToDateTime(TimeOnly.MinValue), System.Data.DbType.Date);
                parameters.Add("To", query.To.ToDateTime(TimeOnly.MinValue), System.Data.DbType.Date);

                var command = new CommandDefinition(
                    sql,
                    parameters,
                    cancellationToken: ct);

                var rows = await connection.QueryAsync<StatusCount>(command);

                return new AppointmentsByStatusResponse(rows.AsList());
            },
            tags: [$"tenant:{tenantId}", "reports"],
            cancellationToken: cancellationToken);

        return Result.Success(response!);
    }
}
