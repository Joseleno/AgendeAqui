using AgendeAqui.Application.Abstractions.Data;
using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Common;
using Dapper;

namespace AgendeAqui.Application.Reports.GetTeamAppointmentsReport;

internal sealed class GetTeamAppointmentsReportQueryHandler(
    ISqlConnectionFactory sqlConnectionFactory,
    ITenantProvider tenantProvider) : IQueryHandler<GetTeamAppointmentsReportQuery, TeamAppointmentsReportResponse>
{
    public async ValueTask<Result<TeamAppointmentsReportResponse>> Handle(
        GetTeamAppointmentsReportQuery query,
        CancellationToken cancellationToken)
    {
        using var connection = await sqlConnectionFactory.CreateConnectionAsync(cancellationToken);
        var tenantId = tenantProvider.GetTenantId();

        // CTE pre-aggregates appointment counts per professional to avoid fan-out
        // when a professional belongs to multiple teams in the period.
        const string teamSql = """
            WITH appointment_counts AS (
                SELECT a.professional_id,
                       CAST(COUNT(*) FILTER (WHERE a.status IN ('Completed','Cancelled','NoShow')) AS integer) AS total,
                       CAST(COUNT(*) FILTER (WHERE a.status = 'Completed') AS integer)  AS completed,
                       CAST(COUNT(*) FILTER (WHERE a.status = 'Cancelled') AS integer)  AS cancelled,
                       CAST(COUNT(*) FILTER (WHERE a.status = 'NoShow')    AS integer)  AS no_show
                FROM appointments a
                WHERE a.tenant_id = @TenantId
                  AND a.date BETWEEN @From AND @To
                GROUP BY a.professional_id
            ),
            team_professionals AS (
                SELECT DISTINCT ON (tm.professional_id) tm.professional_id, tm.team_id
                FROM team_members tm
                WHERE tm.tenant_id = @TenantId
                  AND tm.joined_at <= @To
                  AND (tm.left_at IS NULL OR tm.left_at >= @From)
            )
            SELECT t.id             AS TeamId,
                   t.name           AS TeamName,
                   COALESCE(SUM(ac.total),     0) AS Total,
                   COALESCE(SUM(ac.completed), 0) AS Completed,
                   COALESCE(SUM(ac.cancelled), 0) AS Cancelled,
                   COALESCE(SUM(ac.no_show),   0) AS NoShow
            FROM teams t
            LEFT JOIN team_professionals tp ON tp.team_id = t.id
            LEFT JOIN appointment_counts ac ON ac.professional_id = tp.professional_id
            WHERE t.tenant_id = @TenantId AND t.is_active = true
            GROUP BY t.id, t.name
            ORDER BY Total DESC, t.name
            """;

        // Professionals with no team memberships in period appear here
        const string unassignedSql = """
            WITH assigned_professionals AS (
                SELECT DISTINCT tm.professional_id
                FROM team_members tm
                WHERE tm.tenant_id = @TenantId
                  AND tm.joined_at <= @To
                  AND (tm.left_at IS NULL OR tm.left_at >= @From)
            )
            SELECT p.id                  AS ProfessionalId,
                   p.name                AS ProfessionalName,
                   CAST(COUNT(*) FILTER (WHERE a.status IN ('Completed','Cancelled','NoShow')) AS integer) AS Total,
                   CAST(COUNT(*) FILTER (WHERE a.status = 'Completed') AS integer) AS Completed,
                   CAST(COUNT(*) FILTER (WHERE a.status = 'Cancelled') AS integer) AS Cancelled,
                   CAST(COUNT(*) FILTER (WHERE a.status = 'NoShow')    AS integer) AS NoShow
            FROM professionals p
            INNER JOIN appointments a ON a.professional_id = p.id AND a.tenant_id = @TenantId
                                     AND a.date BETWEEN @From AND @To
            WHERE p.tenant_id = @TenantId
              AND p.id NOT IN (SELECT professional_id FROM assigned_professionals)
            GROUP BY p.id, p.name
            ORDER BY p.name
            """;

        var parameters = new DynamicParameters();
        parameters.Add("TenantId", tenantId);
        parameters.Add("From", query.From.ToDateTime(TimeOnly.MinValue), System.Data.DbType.Date);
        parameters.Add("To", query.To.ToDateTime(TimeOnly.MinValue), System.Data.DbType.Date);

        var cmd = new CommandDefinition(teamSql, parameters, cancellationToken: cancellationToken);
        var unassignedCmd = new CommandDefinition(unassignedSql, parameters, cancellationToken: cancellationToken);

        var teamRows = (await connection.QueryAsync<TeamRow>(cmd)).AsList();
        var unassignedRows = (await connection.QueryAsync<UnassignedRow>(unassignedCmd)).AsList();

        var teamBreakdowns = teamRows.Select(r =>
        {
            var completionRate = r.Total > 0 ? Math.Round((double)r.Completed / r.Total * 100, 1) : 0;
            var cancellationRate = r.Total > 0 ? Math.Round((double)r.Cancelled / r.Total * 100, 1) : 0;
            return new TeamAppointmentBreakdown(r.TeamId, r.TeamName, r.Total, r.Completed, r.Cancelled, r.NoShow, completionRate, cancellationRate);
        }).ToList();

        var unassigned = unassignedRows
            .Select(r => new UnassignedProfessionalBreakdown(r.ProfessionalId, r.ProfessionalName, r.Total, r.Completed, r.Cancelled, r.NoShow))
            .ToList();

        var allCompleted = teamBreakdowns.Sum(t => t.Completed) + unassigned.Sum(u => u.Completed);
        var allCancelled = teamBreakdowns.Sum(t => t.Cancelled) + unassigned.Sum(u => u.Cancelled);
        var allNoShow = teamBreakdowns.Sum(t => t.NoShow) + unassigned.Sum(u => u.NoShow);
        var allTotal = teamBreakdowns.Sum(t => t.Total) + unassigned.Sum(u => u.Total);

        return Result.Success(new TeamAppointmentsReportResponse(
            allTotal, allCompleted, allCancelled, allNoShow,
            teamBreakdowns, unassigned));
    }

    private sealed record TeamRow(Guid TeamId, string TeamName, int Total, int Completed, int Cancelled, int NoShow);
    private sealed record UnassignedRow(Guid ProfessionalId, string ProfessionalName, int Total, int Completed, int Cancelled, int NoShow);
}
