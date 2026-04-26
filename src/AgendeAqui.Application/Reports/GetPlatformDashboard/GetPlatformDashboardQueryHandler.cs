using AgendeAqui.Application.Abstractions.Data;
using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Domain.Common;
using Dapper;

namespace AgendeAqui.Application.Reports.GetPlatformDashboard;

// Uses IPlatformSqlConnectionFactory — bypasses PostgreSQL RLS to read across all tenants.
// Only inject this handler via RequirePlatformOperator policy endpoints.
internal sealed class GetPlatformDashboardQueryHandler(
    IPlatformSqlConnectionFactory platformConnectionFactory) : IQueryHandler<GetPlatformDashboardQuery, PlatformDashboardResponse>
{
    public async ValueTask<Result<PlatformDashboardResponse>> Handle(
        GetPlatformDashboardQuery query,
        CancellationToken cancellationToken)
    {
        using var connection = await platformConnectionFactory.CreateConnectionAsync(cancellationToken);

        const string tenantCountSql = """
            SELECT
                CAST(COUNT(*) AS integer)                                           AS Total,
                CAST(COUNT(*) FILTER (WHERE status = 1) AS integer)                AS Active
            FROM tenants
            """;

        const string tenantMetricsSql = """
            SELECT
                t.id                  AS TenantId,
                t.name                AS TenantName,
                t.slug                AS TenantSlug,
                CAST(COUNT(a.id) FILTER (WHERE a.status IN ('Completed','Cancelled','NoShow')) AS integer) AS Total,
                CAST(COUNT(a.id) FILTER (WHERE a.status = 'Completed')  AS integer) AS Completed,
                CAST(COUNT(a.id) FILTER (WHERE a.status = 'Cancelled')  AS integer) AS Cancelled,
                CAST(COUNT(a.id) FILTER (WHERE a.status = 'NoShow')     AS integer) AS NoShow,
                CAST(COUNT(DISTINCT p.id) FILTER (WHERE p.is_active = true) AS integer) AS ActiveProfessionals
            FROM tenants t
            LEFT JOIN appointments a ON a.tenant_id = t.id
                                     AND a.date BETWEEN @From AND @To
            LEFT JOIN professionals p ON p.tenant_id = t.id
            GROUP BY t.id, t.name, t.slug
            ORDER BY Total DESC, t.name
            """;

        var parameters = new DynamicParameters();
        parameters.Add("From", query.From.ToDateTime(TimeOnly.MinValue), System.Data.DbType.Date);
        parameters.Add("To", query.To.ToDateTime(TimeOnly.MinValue), System.Data.DbType.Date);

        var counts = await connection.QuerySingleAsync<(int Total, int Active)>(
            new CommandDefinition(tenantCountSql, cancellationToken: cancellationToken));

        var rows = (await connection.QueryAsync<TenantRow>(
            new CommandDefinition(tenantMetricsSql, parameters, cancellationToken: cancellationToken))).AsList();

        var summaries = rows.Select(r =>
        {
            var rate = r.Total > 0 ? Math.Round((double)r.Completed / r.Total * 100, 1) : 0;
            return new TenantAppointmentSummary(r.TenantId, r.TenantName, r.TenantSlug,
                r.Total, r.Completed, r.Cancelled, r.NoShow, rate, r.ActiveProfessionals);
        }).ToList();

        return Result.Success(new PlatformDashboardResponse(
            ActiveTenants: counts.Active,
            TotalTenants: counts.Total,
            TotalAppointmentsInPeriod: summaries.Sum(s => s.Total),
            Tenants: summaries));
    }

    private sealed record TenantRow(
        Guid TenantId, string TenantName, string TenantSlug,
        int Total, int Completed, int Cancelled, int NoShow, int ActiveProfessionals);
}
