using AgendeAqui.Application.Abstractions.Data;
using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Common;
using Dapper;

namespace AgendeAqui.Application.Schedules.DetectConflicts;

public sealed class DetectScheduleConflictsQueryHandler(
    ISqlConnectionFactory sqlConnectionFactory,
    ITenantProvider tenantProvider) : IQueryHandler<DetectScheduleConflictsQuery, List<ScheduleConflictResponse>>
{
    public async ValueTask<Result<List<ScheduleConflictResponse>>> Handle(
        DetectScheduleConflictsQuery query,
        CancellationToken cancellationToken)
    {
        using var connection = await sqlConnectionFactory.CreateConnectionAsync(cancellationToken);

        var tenantId = tenantProvider.GetTenantId();

        const string sql = """
            SELECT
                a1.professional_id AS ProfessionalId,
                p.name             AS ProfessionalName,
                a1.id              AS AppointmentId1,
                a2.id              AS AppointmentId2,
                a1.start_time      AS StartTime1,
                a1.end_time        AS EndTime1,
                a2.start_time      AS StartTime2,
                a2.end_time        AS EndTime2,
                c1.name            AS Client1,
                c2.name            AS Client2
            FROM appointments a1
            INNER JOIN appointments a2
                ON  a1.professional_id = a2.professional_id
                AND a1.date            = a2.date
                AND a1.id              < a2.id
                AND a1.start_time      < a2.end_time
                AND a2.start_time      < a1.end_time
            INNER JOIN professionals p  ON p.id  = a1.professional_id
            INNER JOIN clients       c1 ON c1.id = a1.client_id
            INNER JOIN clients       c2 ON c2.id = a2.client_id
            WHERE a1.tenant_id = @TenantId
              AND a2.tenant_id = @TenantId
              AND a1.date      = @Date
              AND a1.status   <> 'Cancelled'
              AND a2.status   <> 'Cancelled'
            ORDER BY p.name, a1.start_time
            """;

        var parameters = new DynamicParameters();
        parameters.Add("TenantId", tenantId);
        parameters.Add("Date", query.Date.ToDateTime(TimeOnly.MinValue), System.Data.DbType.Date);

        var command = new CommandDefinition(sql, parameters, cancellationToken: cancellationToken);

        var rows = await connection.QueryAsync<ConflictRow>(command);

        var response = rows
            .Select(r => new ScheduleConflictResponse(
                r.ProfessionalId,
                r.ProfessionalName,
                r.AppointmentId1,
                r.AppointmentId2,
                r.StartTime1.ToString(@"hh\:mm"),
                r.EndTime1.ToString(@"hh\:mm"),
                r.StartTime2.ToString(@"hh\:mm"),
                r.EndTime2.ToString(@"hh\:mm"),
                r.Client1,
                r.Client2))
            .ToList();

        return Result.Success(response);
    }

    private sealed record ConflictRow(
        Guid ProfessionalId,
        string ProfessionalName,
        Guid AppointmentId1,
        Guid AppointmentId2,
        TimeSpan StartTime1,
        TimeSpan EndTime1,
        TimeSpan StartTime2,
        TimeSpan EndTime2,
        string Client1,
        string Client2);
}
