using AgendeAqui.Application.Abstractions.Data;
using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Common;
using Dapper;
using Microsoft.Extensions.Caching.Hybrid;

namespace AgendeAqui.Application.Reports.GetDashboardOverview;

public sealed class GetDashboardOverviewQueryHandler(
    ISqlConnectionFactory sqlConnectionFactory,
    ITenantProvider tenantProvider,
    HybridCache cache) : IQueryHandler<GetDashboardOverviewQuery, DashboardOverviewResponse>
{
    public async ValueTask<Result<DashboardOverviewResponse>> Handle(
        GetDashboardOverviewQuery query,
        CancellationToken cancellationToken)
    {
        var tenantId = tenantProvider.GetTenantId();

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var dayOfWeek = (int)today.DayOfWeek;
        var daysToMonday = dayOfWeek == 0 ? 6 : dayOfWeek - 1;
        var weekStart = today.AddDays(-daysToMonday);
        var weekEnd = weekStart.AddDays(6);

        var response = await cache.GetOrCreateAsync(
            $"dashboard:{tenantId}:{today}",
            async ct =>
            {
                using var connection = await sqlConnectionFactory.CreateConnectionAsync(ct);

                const string sql = """
                    SELECT
                        CAST((SELECT COUNT(*) FROM appointments WHERE tenant_id = @TenantId AND date = @Today) AS integer) AS AppointmentsToday,
                        CAST((SELECT COUNT(*) FROM appointments WHERE tenant_id = @TenantId AND date BETWEEN @WeekStart AND @WeekEnd) AS integer) AS AppointmentsThisWeek,
                        CAST((SELECT COUNT(*) FROM professionals WHERE tenant_id = @TenantId AND is_active = true) AS integer) AS ActiveProfessionals,
                        CAST((SELECT COUNT(*) FROM clients WHERE tenant_id = @TenantId) AS integer) AS RegisteredClients,
                        CAST((SELECT COUNT(*) FROM appointments WHERE tenant_id = @TenantId AND date = @Today AND status = 'Cancelled') AS integer) AS CancelledToday,
                        CAST((SELECT COUNT(*) FROM appointments WHERE tenant_id = @TenantId AND date = @Today AND status = 'NoShow') AS integer) AS NoShowToday
                    """;

                var parameters = new DynamicParameters();
                parameters.Add("TenantId", tenantId);
                parameters.Add("Today", today.ToDateTime(TimeOnly.MinValue), System.Data.DbType.Date);
                parameters.Add("WeekStart", weekStart.ToDateTime(TimeOnly.MinValue), System.Data.DbType.Date);
                parameters.Add("WeekEnd", weekEnd.ToDateTime(TimeOnly.MinValue), System.Data.DbType.Date);

                var command = new CommandDefinition(
                    sql,
                    parameters,
                    cancellationToken: ct);

                var row = await connection.QuerySingleAsync<DashboardOverviewRow>(command);

                return new DashboardOverviewResponse(
                    AppointmentsToday: row.AppointmentsToday,
                    AppointmentsThisWeek: row.AppointmentsThisWeek,
                    ActiveProfessionals: row.ActiveProfessionals,
                    RegisteredClients: row.RegisteredClients,
                    CancelledToday: row.CancelledToday,
                    NoShowToday: row.NoShowToday);
            },
            tags: [$"tenant:{tenantId}", "dashboard"],
            cancellationToken: cancellationToken);

        return Result.Success(response!);
    }

    private sealed record DashboardOverviewRow(
        int AppointmentsToday,
        int AppointmentsThisWeek,
        int ActiveProfessionals,
        int RegisteredClients,
        int CancelledToday,
        int NoShowToday);
}
