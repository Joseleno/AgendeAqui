using AgendeAqui.Application.Abstractions.Data;
using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Common;
using AgendeAqui.Domain.Professionals;
using Dapper;

namespace AgendeAqui.Application.Reports.GetMyStats;

public sealed class GetMyStatsQueryHandler(
    ISqlConnectionFactory sqlConnectionFactory,
    ICurrentUser currentUser,
    ITenantProvider tenantProvider) : IQueryHandler<GetMyStatsQuery, ProfessionalStatsResponse>
{
    public async ValueTask<Result<ProfessionalStatsResponse>> Handle(
        GetMyStatsQuery query,
        CancellationToken cancellationToken)
    {
        if (currentUser.ProfessionalId is null)
            return Result.Failure<ProfessionalStatsResponse>(ProfessionalErrors.NotFound);

        using var connection = await sqlConnectionFactory.CreateConnectionAsync(cancellationToken);

        var tenantId = tenantProvider.GetTenantId();

        const string sql = """
            SELECT
                CAST(COUNT(*) AS integer) AS Total,
                CAST(COUNT(*) FILTER (WHERE status = 'Completed') AS integer) AS Completed,
                CAST(COUNT(*) FILTER (WHERE status = 'Cancelled') AS integer) AS Cancelled,
                CAST(COUNT(*) FILTER (WHERE status = 'NoShow') AS integer) AS NoShow,
                CAST(COUNT(*) FILTER (WHERE status = 'Scheduled' OR status = 'Confirmed') AS integer) AS Scheduled
            FROM appointments
            WHERE professional_id = @ProfessionalId
                AND tenant_id = @TenantId
                AND date BETWEEN @From AND @To
            """;

        var parameters = new DynamicParameters();
        parameters.Add("ProfessionalId", currentUser.ProfessionalId);
        parameters.Add("TenantId", tenantId);
        parameters.Add("From", query.From.ToDateTime(TimeOnly.MinValue), System.Data.DbType.Date);
        parameters.Add("To", query.To.ToDateTime(TimeOnly.MinValue), System.Data.DbType.Date);

        var command = new CommandDefinition(
            sql,
            parameters,
            cancellationToken: cancellationToken);

        var row = await connection.QuerySingleAsync<StatsRow>(command);

        var completionRate = row.Total > 0
            ? Math.Round((decimal)row.Completed / row.Total * 100, 2)
            : 0m;

        var response = new ProfessionalStatsResponse(
            Total: row.Total,
            Completed: row.Completed,
            Cancelled: row.Cancelled,
            NoShow: row.NoShow,
            Scheduled: row.Scheduled,
            CompletionRate: completionRate);

        return Result.Success(response);
    }

    private sealed record StatsRow(
        int Total,
        int Completed,
        int Cancelled,
        int NoShow,
        int Scheduled);
}
